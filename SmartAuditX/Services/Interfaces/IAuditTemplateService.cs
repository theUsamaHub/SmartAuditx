using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.ViewModels.AuditModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartAuditX.Services.Interfaces
{
    public interface IAuditTemplateService
    {
        // Template operations
        Task<AuditTemplateViewModel> GetTemplateByIdAsync(int templateId, int companyId);
        Task<IEnumerable<AuditTemplateViewModel>> GetTemplatesByCompanyIdAsync(int companyId, bool includePublishedOnly = false);
        Task<int> CreateTemplateAsync(AuditTemplateViewModel model, int companyId);
        Task<bool> UpdateTemplateAsync(AuditTemplateViewModel model, int companyId);
        Task<bool> DeleteTemplateAsync(int templateId, int companyId);
        Task<bool> PublishTemplateAsync(int templateId, int companyId);
        Task<bool> UnpublishTemplateAsync(int templateId, int companyId);

        // Section operations
        Task<AuditTemplateSectionViewModel> AddSectionAsync(int templateId, AuditTemplateSectionViewModel model, int companyId);
        Task<bool> UpdateSectionAsync(int templateId, int sectionId, AuditTemplateSectionViewModel model, int companyId);
        Task<bool> DeleteSectionAsync(int templateId, int sectionId, int companyId);
        Task<bool> ReorderSectionsAsync(int templateId, List<int> sectionIdsInOrder, int companyId);

        // Field operations
        Task<AuditTemplateFieldViewModel> AddFieldAsync(int templateId, int sectionId, AuditTemplateFieldViewModel model, int companyId);
        Task<bool> UpdateFieldAsync(int templateId, int sectionId, int fieldId, AuditTemplateFieldViewModel model, int companyId);
        Task<bool> DeleteFieldAsync(int templateId, int sectionId, int fieldId, int companyId);
        Task<bool> ReorderFieldsAsync(int templateId, int sectionId, List<int> fieldIdsInOrder, int companyId);

        // Field Option operations
        Task<AuditTemplateFieldOptionViewModel> AddOptionAsync(int templateId, int sectionId, int fieldId, AuditTemplateFieldOptionViewModel model, int companyId);
        Task<bool> UpdateOptionAsync(int templateId, int sectionId, int fieldId, int optionId, AuditTemplateFieldOptionViewModel model, int companyId);
        Task<bool> DeleteOptionAsync(int templateId, int sectionId, int fieldId, int optionId, int companyId);
        Task<bool> ReorderOptionsAsync(int templateId, int sectionId, int fieldId, List<int> optionIdsInOrder, int companyId);
    }
}