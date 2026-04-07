using DAL;
using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class LikeRepository
    {
        public List<User> GetLikedUsers(List<Like> likes)
        {
            if (likes == null)
                return new List<User>();

            return likes
                .Where(like => like != null)
                .OrderBy(like => like.Created)
                .Select(like => DB.Users.Get(like.UserId))
                .Where(user => user != null)
                .Select(user => user.Copy())
                .ToList();
        }

        public bool CurrentUserLikes(List<Like> likes, int userId)
        {
            if (likes == null)
                return false;

            return likes.Any(like => like != null && like.UserId == userId);
        }

        public int Count(List<Like> likes)
        {
            if (likes == null)
                return 0;

            return likes.Count(like => like != null);
        }
    }
}
