using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
    public class UserFavoriteIngredients
    {
        public virtual Guid UserFavoriteIngredientsId { get; set; }
        public virtual UserProfiles UserId { get; set; }
        public virtual RecipeIngredients RecipeIngredients { get; set; }

        public class UserFavoriteIngredientsMap : ClassMap<UserFavoriteIngredients>
        {
            public UserFavoriteIngredientsMap()
            {
                Id(x => x.UserFavoriteIngredientsId)
                    .GeneratedBy.GuidComb()
                    .UnsavedValue(Guid.Empty);
                
                References(x => x.UserId).Column("UserId").Not.Nullable();
                References(x => x.RecipeIngredients).Column("RecipeIngredientsId").Not.Nullable();
            }
        }
    }
}