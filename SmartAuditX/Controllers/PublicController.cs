using Microsoft.AspNetCore.Mvc;
using SmartAuditX.Services.Interfaces.CMS;

namespace SmartAuditX.Controllers
{
    public class PublicController : Controller
    {
        private readonly IHeroSectionService _heroSectionService;
        private readonly IFeatureService _featureService;
        private readonly IFaqService _faqService;
        private readonly IAboutUsService _aboutUsService;
        private readonly ITeamMemberService _teamMemberService;
        private readonly IContactInformationService _contactInfoService;
        private readonly IHowItWorksService _howItWorksService;
        private readonly IPlatformModuleService _platformModuleService;
        private readonly ISecurityFeatureService _securityFeatureService;

        public PublicController(
            IHeroSectionService heroSectionService,
            IFeatureService featureService,
            IFaqService faqService,
            IAboutUsService aboutUsService,
            ITeamMemberService teamMemberService,
            IContactInformationService contactInfoService,
            IHowItWorksService howItWorksService,
            IPlatformModuleService platformModuleService,
            ISecurityFeatureService securityFeatureService)
        {
            _heroSectionService = heroSectionService;
            _featureService = featureService;
            _faqService = faqService;
            _aboutUsService = aboutUsService;
            _teamMemberService = teamMemberService;
            _contactInfoService = contactInfoService;
            _howItWorksService = howItWorksService;
            _platformModuleService = platformModuleService;
            _securityFeatureService = securityFeatureService;
        }

        public async Task<IActionResult> Index()
        {
            var heroSections = await _heroSectionService.GetAllAsync();
            var features = await _featureService.GetAllAsync();
            var faqs = await _faqService.GetAllAsync();
            var aboutUs = await _aboutUsService.GetAllAsync();
            var teamMembers = await _teamMemberService.GetAllAsync();
            var contactInfos = await _contactInfoService.GetAllAsync();
            var howItWorks = await _howItWorksService.GetAllAsync();
            var platformModules = await _platformModuleService.GetAllAsync();
            var securityFeatures = await _securityFeatureService.GetAllAsync();

            ViewBag.HeroSection = heroSections.FirstOrDefault(x => x.IsActive);
            ViewBag.Features = features.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
            ViewBag.Faqs = faqs.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
            ViewBag.AboutUs = aboutUs.FirstOrDefault(x => x.IsActive);
            ViewBag.TeamMembers = teamMembers.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
            ViewBag.ContactInfo = contactInfos.FirstOrDefault(x => x.IsActive);
            ViewBag.HowItWorks = howItWorks.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
            ViewBag.PlatformModules = platformModules.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();
            ViewBag.SecurityFeatures = securityFeatures.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToList();

            return View();
        }
    }
}
