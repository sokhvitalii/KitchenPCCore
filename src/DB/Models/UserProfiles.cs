using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
    public class UserProfiles
    {
        public virtual string UserId { get; set; }
        public virtual IList<UserProfilesRatings> Ratings { get; set; }
        // public virtual PantryItem[] Pantry { get; set; }
        public virtual IList<UserFavoriteIngredients> FavoriteIngredients { get; set; }
        public virtual IList<UserBlacklistedIngredients> BlacklistedIngredients { get; set; }
        public virtual Guid? AvoidRecipe { get; set; }
        public virtual RecipeMetadata FavoriteTags { get; set; }
        public virtual RecipeMetadata AllowedTags { get; set; }
        
        
        public class UserProfilesMap : ClassMap<UserProfiles>
        {
            public UserProfilesMap()
            {
                Id(x => x.UserId).UnsavedValue("").Not.Nullable();

                Map(x => x.AvoidRecipe).Nullable();

                HasMany(x => x.Ratings).KeyColumn("UserId");
                HasMany(x => x.FavoriteIngredients).KeyColumn("UserId");
                HasMany(x => x.BlacklistedIngredients).KeyColumn("UserId");
                
                References(x => x.AllowedTags).Column("AllowedRecipeMetadataId").Nullable();
                References(x => x.FavoriteTags).Column("FavoriteRecipeMetadataId").Nullable();
            }
        }
    }
}