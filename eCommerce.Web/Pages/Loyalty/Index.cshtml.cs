using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Pages.Loyalty
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoyaltyPoint? LoyaltyInfo { get; set; }
        public List<PointTransaction> Transactions { get; set; } = new();
        public string TierIcon { get; set; } = "";
        public string TierColor { get; set; } = "";
        public string NextTier { get; set; } = "";
        public int PointsNeeded { get; set; }
        public double ProgressPercent { get; set; }

        public IndexModel(ILoyaltyService loyaltyService, UserManager<ApplicationUser> userManager)
        {
            _loyaltyService = loyaltyService;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            LoyaltyInfo = await _loyaltyService.GetOrCreateLoyaltyPointAsync(user.Id);
            if (LoyaltyInfo == null) return;

            Transactions = await _loyaltyService.GetTransactionHistoryAsync(user.Id, 20);
            TierIcon = _loyaltyService.GetTierIcon(LoyaltyInfo.MembershipTier);
            TierColor = _loyaltyService.GetTierColor(LoyaltyInfo.MembershipTier);

            var nextTierInfo = _loyaltyService.GetNextTierInfo(LoyaltyInfo.LifetimePoints);
            NextTier = nextTierInfo.NextTier;
            PointsNeeded = nextTierInfo.PointsNeeded;
            ProgressPercent = nextTierInfo.ProgressPercent;
        }
    }
}
