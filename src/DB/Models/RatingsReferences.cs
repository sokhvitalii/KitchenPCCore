using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
    public class UserProfilesRatings
    {
        
        public virtual Guid UserProfilesRatingsId { get; set; }
        public virtual UserProfiles UserId { get; set; }
        public virtual RecipeRatings Ratings { get; set; }

        public class UserProfilesRatingsMap : ClassMap<UserProfilesRatings>
        {
            public UserProfilesRatingsMap()
            {
                Id(x => x.UserProfilesRatingsId)
                    .GeneratedBy.GuidComb()
                    .UnsavedValue(Guid.Empty);
                
                References(x => x.UserId).Column("UserId").Not.Nullable();
                References(x => x.Ratings).Column("RatingsId").Not.Nullable();
            }
        }
    }
}