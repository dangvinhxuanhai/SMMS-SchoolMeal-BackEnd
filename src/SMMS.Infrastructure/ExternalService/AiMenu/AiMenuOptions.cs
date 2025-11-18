using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Infrastructure.ExternalService.AiMenu;
public class AiMenuOptions
{
    public const string SectionName = "AiMenuService";

    public string BaseUrl { get; set; } = "http://localhost:8000";       // vd: http://localhost:8000
    public string RecommendEndpoint { get; set; } = "/menu/recommend";
}
