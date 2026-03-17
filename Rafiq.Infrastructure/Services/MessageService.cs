using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Common;
using Rafiq.Application.DTOs.Messages;
using Rafiq.Application.Exceptions;
using Rafiq.Application.Interfaces.Common;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Application.Interfaces.Services;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Identity;

namespace Rafiq.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public MessageService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<MessageDto> SendAsync(SendMessageRequestDto request, CancellationToken cancellationToken = default)
    {
        var senderId = GetUserId();
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(request.ChildId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        ValidateMessagingPermission(child, senderId, request.ReceiverUserId);

        var message = new Message
        {
            ChildId = request.ChildId,
            SenderUserId = senderId,
            ReceiverUserId = request.ReceiverUserId,
            Content = request.Content,
            IsRead = false
        };

        await _unitOfWork.Messages.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<MessageDto>(message);
    }

    public async Task<PagedResult<MessageDto>> GetConversationByChildAsync(
        int childId,
        PagedRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var child = await _unitOfWork.Children.GetByIdWithDetailsAsync(childId, cancellationToken)
            ?? throw new NotFoundException("Child was not found.");

        EnsureConversationAccess(child, userId);

        var messages = await _unitOfWork.Messages.GetConversationByChildAsync(
            childId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var totalCount = await _unitOfWork.Messages.CountConversationByChildAsync(childId, cancellationToken);

        var items = messages.Select(_mapper.Map<MessageDto>).ToList();

        return new PagedResult<MessageDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default)
    {
        var message = await _unitOfWork.Messages.GetByIdAsync(messageId, cancellationToken)
            ?? throw new NotFoundException("Message was not found.");

        var userId = GetUserId();

        if (!_currentUser.IsInRole(RoleNames.Admin) && message.ReceiverUserId != userId)
        {
            throw new ForbiddenException("Only the receiver can mark this message as read.");
        }

        message.IsRead = true;
        _unitOfWork.Messages.Update(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private void ValidateMessagingPermission(Child child, int senderId, int receiverId)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Parent))
        {
            if (child.ParentProfile.UserId != senderId)
            {
                throw new ForbiddenException("Parent can only message for their own child.");
            }

            if (child.SpecialistProfile is null)
            {
                throw new BadRequestException("Child is not assigned to a specialist.");
            }

            if (receiverId != child.SpecialistProfile.UserId)
            {
                throw new BadRequestException("Parent can only message the assigned specialist.");
            }

            return;
        }

        if (_currentUser.IsInRole(RoleNames.Specialist))
        {
            if (child.SpecialistProfile?.UserId != senderId)
            {
                throw new ForbiddenException("Specialist can only message for assigned children.");
            }

            if (receiverId != child.ParentProfile.UserId)
            {
                throw new BadRequestException("Specialist can only message the child's parent.");
            }

            return;
        }

        throw new ForbiddenException("You are not allowed to send messages.");
    }

    private void EnsureConversationAccess(Child child, int userId)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Parent) && child.ParentProfile.UserId == userId)
        {
            return;
        }

        if (_currentUser.IsInRole(RoleNames.Specialist) && child.SpecialistProfile?.UserId == userId)
        {
            return;
        }

        throw new ForbiddenException("You are not allowed to access this conversation.");
    }

    private int GetUserId()
    {
        return _currentUser.UserId ?? throw new UnauthorizedException("Current user is not authenticated.");
    }
}
