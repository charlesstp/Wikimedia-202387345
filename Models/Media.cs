using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public enum MediaSortBy { Title, PublishDate, Likes }

    public class Media : Record
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string YoutubeId { get; set; }
        public DateTime PublishDate { get; set; } = DateTime.Now;

        public int OwnerId { get; set; } = 1;
        public bool Shared { get; set; } = true;
        public List<int> LikedBy { get; set; } = new List<int>();
        public List<Like> Likes { get; set; } = new List<Like>();
        [JsonIgnore]
        public User Owner => DB.Users.Get(OwnerId).Copy();

        public override bool IsValid()
        {
            if (!HasRequiredLength(Title, 1)) return false;
            if (!HasRequiredLength(Category, 1)) return false;
            if (!HasRequiredLength(Description, 1)) return false;
            if (DB.Medias.ToList().Where(m => m.YoutubeId == YoutubeId && m.Id != Id).Any()) return false;
            return true;
        }

        [JsonIgnore]
        public List<User> LikedUsers
        {
            get
            {
                return OrderedLikes
                    .Select(like => DB.Users.Get(like.UserId))
                    .Where(u => u != null)
                    .Select(u => u.Copy())
                    .ToList();
            }
        }
        [JsonIgnore]
        public int LikesCount
        {
            get
            {
                return OrderedLikes.Count;
            }
        }

        [JsonIgnore]
        public bool CurrentUserLikes
        {
            get
            {
                if (User.ConnectedUser == null) return false;
                return OrderedLikes.Any(like => like.UserId == User.ConnectedUser.Id);
            }
        }

        [JsonIgnore]
        public List<Like> OrderedLikes
        {
            get
            {
                if (Likes == null) Likes = new List<Like>();
                if (LikedBy == null) LikedBy = new List<int>();

                if (Likes.Count == 0 && LikedBy.Count > 0)
                {
                    DateTime baseTime = PublishDate == default(DateTime) ? DateTime.Now : PublishDate;
                    for (int i = 0; i < LikedBy.Count; i++)
                    {
                        if (!Likes.Any(like => like.UserId == LikedBy[i]))
                        {
                            Likes.Add(new Like
                            {
                                UserId = LikedBy[i],
                                Created = baseTime.AddMilliseconds(i)
                            });
                        }
                    }
                }

                Likes = Likes
                    .Where(like => like != null)
                    .GroupBy(like => like.UserId)
                    .Select(group => group.OrderBy(like => like.Created).First())
                    .OrderBy(like => like.Created)
                    .ToList();

                LikedBy = Likes.Select(like => like.UserId).ToList();
                return Likes;
            }
        }

        public void RemoveUserLikes(int userId)
        {
            if (Likes == null) Likes = new List<Like>();
            if (LikedBy == null) LikedBy = new List<int>();

            Likes.RemoveAll(like => like != null && like.UserId == userId);
            LikedBy.RemoveAll(id => id == userId);
        }
    }
}
