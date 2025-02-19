using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Common.Mappings
{
    public class ServicePackageMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ServicePackage, ServicePackageDto>()
                .Map(dest => dest.TotalSubscriptions, src => src.SubscribedUserIds.Count);
        }
    }
}
