
using WebAPI.Interfaces;

namespace WebAPI.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ILikesRepository _likesRepository;
        private readonly DataContext _context;
        public UnitOfWork(DataContext context, IUserRepository userRepository, IMessageRepository messageRepository, ILikesRepository likesRepository) 
        {
            _context = context;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _likesRepository = likesRepository;
        }

        public IUserRepository UserRepository { get { return _userRepository; } }

        public IMessageRepository MessageRepository { get { return _messageRepository; } }

        public ILikesRepository LikesRepository { get { return _likesRepository; } }

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
