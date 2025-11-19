using System.ComponentModel.DataAnnotations;      
using System.ComponentModel.DataAnnotations.Schema; 

namespace eCommerce.Core.Entities
{
    [Table("UserPushSubscriptions")] 
    public class UserPushSubscription
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        [MaxLength(450)] 
        public string UserId { get; set; } 

        [Required]
        public string Endpoint { get; set; } 

        [Required]
        [MaxLength(255)] 
        public string P256dh { get; set; } 

        [Required]
        [MaxLength(100)] 
        public string Auth { get; set; } 
    }
}