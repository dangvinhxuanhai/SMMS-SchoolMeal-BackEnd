using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using SMMS.Domain.Entities.billing;
using SMMS.Domain.Entities.school;

namespace SMMS.WebAPI.Configurations
{
    public static class ODataConfig
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // Đăng ký các Entity mà bạn muốn expose qua OData
            builder.EntitySet<School>("Schools");
            builder.EntitySet<Student>("Students");
            builder.EntitySet<Invoice>("Invoices");

            return builder.GetEdmModel();
        }
    }
}
