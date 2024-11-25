using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Helpers;

namespace WebAPI.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message?> GetMessage(int id);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string RecipientUsername);
        Task<bool> SaveAllAsync();
    }
}
