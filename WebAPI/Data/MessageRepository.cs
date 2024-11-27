
using AutoMapper;
using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Helpers;
using WebAPI.Interfaces;
using WebAPI.SignalR;

namespace WebAPI.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper, IHubContext<PresenceHub> presenceHub) 
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection?> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group?> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message?> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group?> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(x => x.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.RecipientDeleted),
                "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && !x.SenderDeleted),
                _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && !x.RecipientDeleted)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Where(x => 
                    x.RecipientUsername == currentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername || 
                    x.SenderUsername == currentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUsername
                )
                .OrderBy(x => x.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = messages.Where(x => x.DateRead == null && x.RecipientUsername == currentUsername).ToList();

            if (unreadMessages.Count != 0)
            {
                unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
                await _context.SaveChangesAsync();
            }

            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
