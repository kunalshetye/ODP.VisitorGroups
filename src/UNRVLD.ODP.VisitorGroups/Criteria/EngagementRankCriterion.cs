﻿using EPiServer.Personalization.VisitorGroups;

#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Http;
#elif NET461_OR_GREATER
using System.Web;
using EPiServer.ServiceLocation;
#endif

using System.Security.Principal;

namespace UNRVLD.ODP.VisitorGroups.Criteria
{
    [VisitorGroupCriterion(
        Category = "Data platform",
        Description = "Customer engagement rank from 0 to 100 (calculated every 24 hours)",
        DisplayName = "Engagement rank"
    )]
    public class EngagementRankCriterion : OdpCriterionBase<EngagementRankCriterionModel>
    {
        private readonly OdpVisitorGroupOptions _optionValues;
        private readonly ICustomerDataRetriever _customerDataRetriever;

#if NET5_0_OR_GREATER
        public EngagementRankCriterion(OdpVisitorGroupOptions optionValues, 
                ICustomerDataRetriever customerDataRetriever,
                IODPUserProfile odpUserProfile)
            : base(odpUserProfile)
        {
            _optionValues = optionValues;
            _customerDataRetriever = customerDataRetriever;
        }
#elif NET461_OR_GREATER
        public EngagementRankCriterion()
        {
            _customerDataRetriever = ServiceLocator.Current.GetInstance<ICustomerDataRetriever>();
            _optionValues = ServiceLocator.Current.GetInstance<OdpVisitorGroupOptions>();
            OdpUserProfile = ServiceLocator.Current.GetInstance<IODPUserProfile>();
        }
#endif

        protected override bool IsMatchInner(IPrincipal principal, string vuidValue)
        {
            try
            {
                if (_optionValues.IsConfigured == false)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(vuidValue))
                {
                    var customer = _customerDataRetriever.GetCustomerInfo(vuidValue);
                    if (customer == null)
                    {
                        return false;
                    }

                    return CompareMe(customer.Insights?.EngagementRank, Model.Comparison);
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private bool CompareMe(decimal? value, string comparison)
        {
            if (value == null)
            {
                return false;
            }

            switch (comparison)
            {
                case "LessThan":
                    return value < Model.EngagementRank;
                case "EqualTo":
                    return value == Model.EngagementRank;
                case "GreaterThan":
                    return value > Model.EngagementRank;
                default:
                    return false;
            }
        }
    }
}