using Cofoundry.Core.DependencyInjection;
using Cofoundry.Domain.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Plugins.Azure
{
    public class AzureDependencyRegistration : IDependencyRegistration
    {
        public void Register(IContainerRegister container)
        {
            if (container.Configuration.IsAzurePluginEnabled())
            {
                var delayedRegistrationOptions = RegistrationOptions.Override();

                container
                    .RegisterType<IFileStoreService, AzureBlobFileService>(delayedRegistrationOptions)
                    ;
            }
        }
    }
}
