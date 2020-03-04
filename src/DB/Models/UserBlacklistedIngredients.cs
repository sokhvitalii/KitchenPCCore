using System;
using FluentNHibernate.Mapping;

namespace KitchenPC.DB.Models
{
    public class UserBlacklistedIngredients
    {
              
        public virtual Guid UserBlacklistedIngredientsId { get; set; }
        public virtual UserProfiles UserId { get; set; }
        public virtual RecipeIngredients RecipeIngredients { get; set; }

        public class UserBlacklistedIngredientsMap : ClassMap<UserBlacklistedIngredients>
        {
            public UserBlacklistedIngredientsMap()
            {
                Id(x => x.UserBlacklistedIngredientsId)
                    .GeneratedBy.GuidComb()
                    .UnsavedValue(Guid.Empty);
                
                References(x => x.UserId).Column("UserId").Not.Nullable();
                References(x => x.RecipeIngredients).Column("RecipeIngredientsId").Not.Nullable();
            }
        }
    }
}