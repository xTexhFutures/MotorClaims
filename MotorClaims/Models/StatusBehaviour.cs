using CORE.DTOs.APIs.MotorClaim;
using CORE.DTOs.MotorClaim.Claims;

namespace MotorClaims.Models
{
    public static class StatusBehaviour
    {
        public static void ClaimStatus(int ClaimId, AppSettings _appSettings)
        {
            MainSearchMC mainSearchMC = new MainSearchMC()
            {
                Id = ClaimId
            };
            SetupClaimsRequestcs setupClaimsRequestcs = new SetupClaimsRequestcs()
            {
                TransactionType = CORE.Extensions.ClaimTransactionType.LoadClaim,
                Request = mainSearchMC
            };
            var claim = Helpers.ExcutePostAPI<List<Claims>>(setupClaimsRequestcs, _appSettings.APIHubPrefix + "api/MotorClaim/ClaimsTransactions");

            Claims claims = new Claims();
            claims = claim!=null? claim.FirstOrDefault():new Claims();
            if (claims != null)
            {
              
            }

        }
    }
}
