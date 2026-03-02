using Car_Project.Data;
using Car_Project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Car_Project.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;

        // userId ? connectionId mapping (in-memory; fine for single-server)
        private static readonly Dictionary<string, string> _connections = new();

        public ChatHub(UserManager<AppUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_connections)
                    _connections[user.Id] = Context.ConnectionId;

                // Tell everyone this user is online
                await Clients.All.SendAsync("UserOnline", user.Id, user.FullName);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = await _userManager.GetUserAsync(Context.User!);
            if (user != null)
            {
                lock (_connections)
                    _connections.Remove(user.Id);

                await Clients.All.SendAsync("UserOffline", user.Id);
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send a private message to a specific user.
        /// </summary>
        public async Task SendPrivateMessage(string receiverId, string messageText)
        {
            var sender = await _userManager.GetUserAsync(Context.User!);
            if (sender == null || string.IsNullOrWhiteSpace(messageText)) return;

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null) return;

            // Persist
            var msg = new ChatMessage
            {
                SenderId   = sender.Id,
                SenderName = sender.FullName,
                ReceiverId = receiverId,
                Message    = messageText.Trim(),
                SentAt     = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);
            await _db.SaveChangesAsync();

            var payload = new
            {
                id         = msg.Id,
                senderId   = sender.Id,
                senderName = sender.FullName,
                message    = msg.Message,
                sentAt     = msg.SentAt.ToString("HH:mm")
            };

            // Send to receiver (if online)
            string? receiverConn;
            lock (_connections)
                _connections.TryGetValue(receiverId, out receiverConn);

            if (receiverConn != null)
                await Clients.Client(receiverConn).SendAsync("ReceiveMessage", payload);

            // Echo back to sender
            await Clients.Caller.SendAsync("ReceiveMessage", payload);
        }

        /// <summary>
        /// Returns the list of users this user can chat with:
        /// - SuperAdmin
        /// - Agents
        /// - Car sellers (Users who have cars listed) — for now all non-current users of role User/Agent/SuperAdmin
        /// </summary>
        public async Task GetChatUsers()
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            // Get all users except current
            var allUsers = _userManager.Users
                .Where(u => u.Id != me.Id)
                .ToList();

            var result = new List<object>();
            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                // Only include SuperAdmin, Admin, Agent, or User (not hidden roles)
                if (roles.Any(r => new[] { "SuperAdmin", "Admin", "Agent", "User" }.Contains(r)))
                {
                    bool online;
                    lock (_connections)
                        online = _connections.ContainsKey(u.Id);

                    result.Add(new
                    {
                        id     = u.Id,
                        name   = u.FullName,
                        role   = roles.FirstOrDefault() ?? "User",
                        online = online
                    });
                }
            }
            await Clients.Caller.SendAsync("ChatUserList", result);
        }

        /// <summary>
        /// Load chat history between caller and another user.
        /// </summary>
        public async Task LoadHistory(string otherUserId)
        {
            var me = await _userManager.GetUserAsync(Context.User!);
            if (me == null) return;

            var history = _db.ChatMessages
                .Where(m =>
                    (m.SenderId == me.Id    && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == me.Id))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id         = m.Id,
                    senderId   = m.SenderId,
                    senderName = m.SenderName,
                    message    = m.Message,
                    sentAt     = m.SentAt.ToString("HH:mm"),
                    isMe       = m.SenderId == me.Id
                })
                .ToList();

            await Clients.Caller.SendAsync("ChatHistory", history);
        }
    }
}
