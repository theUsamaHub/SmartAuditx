using SmartAuditX.Data;
using SmartAuditX.Models.AuditModule;
using SmartAuditX.Models.ViewModels.AuditModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class AuditTemplateService : IAuditTemplateService
    {
        private readonly ApplicationDbContext _context;

        public AuditTemplateService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper methods for conversion between entities and view models

        private AuditTemplateViewModel MapToViewModel(AuditTemplate template)
        {
            if (template == null) return null;

            var viewModel = new AuditTemplateViewModel
            {
                AuditTemplateId = template.AuditTemplateId,
                Title = template.Title,
                Description = template.Description,
                IsScoringEnabled = template.IsScoringEnabled,
                Version = template.Version,
                IsPublished = template.IsPublished,
                Sections = new List<AuditTemplateSectionViewModel>()
            };

            // Load sections from hierarchical model
            if (template.Sections != null)
            {
                foreach (var section in template.Sections.OrderBy(s => s.SortOrder))
                {
                    var sectionViewModel = new AuditTemplateSectionViewModel
                    {
                        Id = section.Id,
                        Title = section.Title,
                        SortOrder = section.SortOrder,
                        Fields = new List<AuditTemplateFieldViewModel>()
                    };

                    // Load fields for this section
                    if (section.Fields != null)
                    {
                        foreach (var field in section.Fields.OrderBy(f => f.SortOrder))
                        {
                            var fieldViewModel = new AuditTemplateFieldViewModel
                            {
                                Id = field.Id,
                                QuestionText = field.QuestionText,
                                ItemType = field.ItemType,
                                IsRequired = field.IsRequired,
                                Weightage = field.Weightage,
                                Options = new List<AuditTemplateFieldOptionViewModel>()
                            };

                            // Load options for this field
                            if (field.Options != null)
                            {
                                foreach (var option in field.Options.OrderBy(o => o.SortOrder))
                                {
                                    fieldViewModel.Options.Add(new AuditTemplateFieldOptionViewModel
                                    {
                                        Id = option.Id,
                                        Text = option.Text,
                                        Value = option.Value,
                                        SortOrder = option.SortOrder
                                    });
                                }
                            }

                            sectionViewModel.Fields.Add(fieldViewModel);
                        }
                    }

                    viewModel.Sections.Add(sectionViewModel);
                }
            }

            return viewModel;
        }

        private AuditTemplate MapToEntity(AuditTemplateViewModel viewModel)
        {
            if (viewModel == null) return null;

            var template = new AuditTemplate
            {
                AuditTemplateId = viewModel.AuditTemplateId,
                Title = viewModel.Title,
                Description = viewModel.Description,
                IsScoringEnabled = viewModel.IsScoringEnabled,
                Version = viewModel.Version,
                IsPublished = viewModel.IsPublished,
                Sections = new List<AuditTemplateSection>()
            };

            // Map sections
            if (viewModel.Sections != null)
            {
                foreach (var sectionViewModel in viewModel.Sections)
                {
                    var section = new AuditTemplateSection
                    {
                        Id = sectionViewModel.Id,
                        AuditTemplateId = template.AuditTemplateId,
                        Title = sectionViewModel.Title,
                        SortOrder = sectionViewModel.SortOrder,
                        Fields = new List<AuditTemplateField>()
                    };

                    // Map fields
                    if (sectionViewModel.Fields != null)
                    {
                        foreach (var fieldViewModel in sectionViewModel.Fields)
                        {
                            var field = new AuditTemplateField
                            {
                                Id = fieldViewModel.Id,
                                AuditTemplateSectionId = section.Id,
                                QuestionText = fieldViewModel.QuestionText,
                                ItemType = fieldViewModel.ItemType,
                                IsRequired = fieldViewModel.IsRequired,
                                Weightage = fieldViewModel.Weightage,
                                Options = new List<AuditTemplateFieldOption>()
                            };

                            // Map options
                            if (fieldViewModel.Options != null)
                            {
                                foreach (var optionViewModel in fieldViewModel.Options)
                                {
                                    field.Options.Add(new AuditTemplateFieldOption
                                    {
                                        Id = optionViewModel.Id,
                                        AuditTemplateFieldId = field.Id,
                                        Text = optionViewModel.Text,
                                        Value = optionViewModel.Value,
                                        SortOrder = optionViewModel.SortOrder
                                    });
                                }
                            }

                            section.Fields.Add(field);
                        }
                    }

                    template.Sections.Add(section);
                }
            }

            return template;
        }

        // Template operations
        public async Task<AuditTemplateViewModel> GetTemplateByIdAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Options)
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            return MapToViewModel(template);
        }

        public async Task<IEnumerable<AuditTemplateViewModel>> GetTemplatesByCompanyIdAsync(int companyId, bool includePublishedOnly = false)
        {
            IQueryable<AuditTemplate> query = _context.AuditTemplates
                .Where(t => t.CompanyId == companyId)
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Options);

            if (includePublishedOnly)
            {
                query = query.Where(t => t.IsPublished);
            }

            var templates = await query.ToListAsync();
            return templates.Select(MapToViewModel);
        }

        public async Task<int> CreateTemplateAsync(AuditTemplateViewModel model, int companyId)
        {
            var template = MapToEntity(model);
            template.CompanyId = companyId;
            template.CreatedAt = DateTime.UtcNow;
            template.IsActive = true;

            _context.AuditTemplates.Add(template);
            await _context.SaveChangesAsync();

            return template.AuditTemplateId;
        }

        public async Task<bool> UpdateTemplateAsync(AuditTemplateViewModel model, int companyId)
        {
            var existingTemplate = await _context.AuditTemplates
                .Include(t => t.Sections)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(f => f.Options)
                .FirstOrDefaultAsync(t => t.AuditTemplateId == model.AuditTemplateId && t.CompanyId == companyId);

            if (existingTemplate == null)
                return false;

            // If the template is published, create a new version
            if (existingTemplate.IsPublished)
            {
                var newVersion = MapToEntity(model);
                newVersion.AuditTemplateId = 0;
                newVersion.CompanyId = companyId;
                newVersion.Version = existingTemplate.Version + 1;
                newVersion.IsPublished = false;
                newVersion.IsActive = true;
                newVersion.CreatedAt = DateTime.UtcNow;

                _context.AuditTemplates.Add(newVersion);
                await _context.SaveChangesAsync();
                return true;
            }

            // Update draft template
            existingTemplate.Title = model.Title;
            existingTemplate.Description = model.Description;
            existingTemplate.IsScoringEnabled = model.IsScoringEnabled;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            // Remove existing sections and add new ones
            _context.AuditTemplateSections.RemoveRange(existingTemplate.Sections);
            existingTemplate.Sections = MapToEntity(model)?.Sections ?? new List<AuditTemplateSection>();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId &&
                                          t.CompanyId == companyId);

            if (template == null)
            {
                return false;
            }

            template.IsDeleted = true;
            template.DeletedAt = DateTime.UtcNow;

            var affectedRows = await _context.SaveChangesAsync();

            return affectedRows > 0;
        }
        public async Task<bool> PublishTemplateAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            if (template == null)
                return false;

            if (template.IsPublished)
                return true;

            template.IsPublished = true;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnpublishTemplateAsync(int templateId, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            if (template == null)
                return false;

            if (!template.IsPublished)
                return true;

            template.IsPublished = false;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        // Section operations
        public async Task<AuditTemplateSectionViewModel> AddSectionAsync(int templateId, AuditTemplateSectionViewModel model, int companyId)
        {
            var template = await _context.AuditTemplates
                .FirstOrDefaultAsync(t => t.AuditTemplateId == templateId && t.CompanyId == companyId);

            if (template == null)
                return null;

            var section = new AuditTemplateSection
            {
                AuditTemplateId = templateId,
                Title = model.Title,
                SortOrder = template.Sections.Count,
                Fields = new List<AuditTemplateField>()
            };

            _context.AuditTemplateSections.Add(section);
            await _context.SaveChangesAsync();

            return new AuditTemplateSectionViewModel
            {
                Id = section.Id,
                Title = section.Title,
                SortOrder = section.SortOrder,
                Fields = new List<AuditTemplateFieldViewModel>()
            };
        }

        public async Task<bool> UpdateSectionAsync(int templateId, int sectionId, AuditTemplateSectionViewModel model, int companyId)
        {
            var section = await _context.AuditTemplateSections
                .Include(s => s.AuditTemplate)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.AuditTemplate.CompanyId == companyId);

            if (section == null)
                return false;

            section.Title = model.Title;
            section.SortOrder = model.SortOrder;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSectionAsync(int templateId, int sectionId, int companyId)
        {
            var section = await _context.AuditTemplateSections
                .Include(s => s.AuditTemplate)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.AuditTemplate.CompanyId == companyId);

            if (section == null)
                return false;

            _context.AuditTemplateSections.Remove(section);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReorderSectionsAsync(int templateId, List<int> sectionIdsInOrder, int companyId)
        {
            var sections = await _context.AuditTemplateSections
                .Include(s => s.AuditTemplate)
                .Where(s => s.AuditTemplateId == templateId && s.AuditTemplate.CompanyId == companyId)
                .ToListAsync();

            if (sections == null || !sections.Any())
                return false;

            for (int i = 0; i < sectionIdsInOrder.Count; i++)
            {
                var section = sections.FirstOrDefault(s => s.Id == sectionIdsInOrder[i]);
                if (section != null)
                {
                    section.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Field operations
        public async Task<AuditTemplateFieldViewModel> AddFieldAsync(int templateId, int sectionId, AuditTemplateFieldViewModel model, int companyId)
        {
            var section = await _context.AuditTemplateSections
                .Include(s => s.AuditTemplate)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.AuditTemplate.CompanyId == companyId);

            if (section == null)
                return null;

            var field = new AuditTemplateField
            {
                AuditTemplateSectionId = sectionId,
                QuestionText = model.QuestionText,
                ItemType = model.ItemType,
                IsRequired = model.IsRequired,
                Weightage = model.Weightage,
                SortOrder = section.Fields.Count,
                Options = new List<AuditTemplateFieldOption>()
            };

            _context.AuditTemplateFields.Add(field);
            await _context.SaveChangesAsync();

            // Save options if provided (for Dropdown/Checkbox field types)
            var savedOptions = new List<AuditTemplateFieldOptionViewModel>();
            if (model.Options != null && model.Options.Any())
            {
                for (int i = 0; i < model.Options.Count; i++)
                {
                    var option = new AuditTemplateFieldOption
                    {
                        AuditTemplateFieldId = field.Id,
                        Text = model.Options[i].Text,
                        SortOrder = i
                    };
                    _context.AuditTemplateFieldOptions.Add(option);
                    savedOptions.Add(new AuditTemplateFieldOptionViewModel
                    {
                        Id = option.Id,
                        Text = option.Text,
                        SortOrder = option.SortOrder
                    });
                }
                await _context.SaveChangesAsync();
            }

            return new AuditTemplateFieldViewModel
            {
                Id = field.Id,
                QuestionText = field.QuestionText,
                ItemType = field.ItemType,
                IsRequired = field.IsRequired,
                Weightage = field.Weightage,
                Options = savedOptions
            };
        }

        public async Task<bool> UpdateFieldAsync(int templateId, int sectionId, int fieldId, AuditTemplateFieldViewModel model, int companyId)
        {
            var field = await _context.AuditTemplateFields
                .Include(f => f.AuditTemplateSection)
                    .ThenInclude(s => s.AuditTemplate)
                .Include(f => f.Options)
                .FirstOrDefaultAsync(f => f.Id == fieldId && f.AuditTemplateSection.AuditTemplate.CompanyId == companyId);

            if (field == null)
                return false;

            field.QuestionText = model.QuestionText;
            field.ItemType = model.ItemType;
            field.IsRequired = model.IsRequired;
            field.Weightage = model.Weightage;
            field.SortOrder = model.SortOrder;

            // Update options: remove existing, add new ones
            if (model.Options != null)
            {
                _context.AuditTemplateFieldOptions.RemoveRange(field.Options);

                for (int i = 0; i < model.Options.Count; i++)
                {
                    var option = new AuditTemplateFieldOption
                    {
                        AuditTemplateFieldId = field.Id,
                        Text = model.Options[i].Text,
                        SortOrder = i
                    };
                    _context.AuditTemplateFieldOptions.Add(option);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFieldAsync(int templateId, int sectionId, int fieldId, int companyId)
        {
            var field = await _context.AuditTemplateFields
                .Include(f => f.AuditTemplateSection)
                    .ThenInclude(s => s.AuditTemplate)
                .FirstOrDefaultAsync(f => f.Id == fieldId && f.AuditTemplateSection.AuditTemplate.CompanyId == companyId);

            if (field == null)
                return false;

            _context.AuditTemplateFields.Remove(field);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReorderFieldsAsync(int templateId, int sectionId, List<int> fieldIdsInOrder, int companyId)
        {
            var fields = await _context.AuditTemplateFields
                .Include(f => f.AuditTemplateSection)
                    .ThenInclude(s => s.AuditTemplate)
                .Where(f => f.AuditTemplateSectionId == sectionId && f.AuditTemplateSection.AuditTemplate.CompanyId == companyId)
                .ToListAsync();

            if (fields == null || !fields.Any())
                return false;

            for (int i = 0; i < fieldIdsInOrder.Count; i++)
            {
                var field = fields.FirstOrDefault(f => f.Id == fieldIdsInOrder[i]);
                if (field != null)
                {
                    field.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Field Option operations
        public async Task<AuditTemplateFieldOptionViewModel> AddOptionAsync(int templateId, int sectionId, int fieldId, AuditTemplateFieldOptionViewModel model, int companyId)
        {
            var field = await _context.AuditTemplateFields
                .Include(f => f.AuditTemplateSection)
                    .ThenInclude(s => s.AuditTemplate)
                .FirstOrDefaultAsync(f => f.Id == fieldId && f.AuditTemplateSection.AuditTemplate.CompanyId == companyId);

            if (field == null)
                return null;

            var option = new AuditTemplateFieldOption
            {
                AuditTemplateFieldId = fieldId,
                Text = model.Text,
                Value = model.Value,
                SortOrder = field.Options.Count
            };

            _context.AuditTemplateFieldOptions.Add(option);
            await _context.SaveChangesAsync();

            return new AuditTemplateFieldOptionViewModel
            {
                Id = option.Id,
                Text = option.Text,
                Value = option.Value,
                SortOrder = option.SortOrder
            };
        }

        public async Task<bool> UpdateOptionAsync(int templateId, int sectionId, int fieldId, int optionId, AuditTemplateFieldOptionViewModel model, int companyId)
        {
            var option = await _context.AuditTemplateFieldOptions
                .Include(o => o.AuditTemplateField)
                    .ThenInclude(f => f.AuditTemplateSection)
                        .ThenInclude(s => s.AuditTemplate)
                .FirstOrDefaultAsync(o => o.Id == optionId && o.AuditTemplateField.AuditTemplateSection.AuditTemplate.CompanyId == companyId);

            if (option == null)
                return false;

            option.Text = model.Text;
            option.Value = model.Value;
            option.SortOrder = model.SortOrder;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOptionAsync(int templateId, int sectionId, int fieldId, int optionId, int companyId)
        {
            var option = await _context.AuditTemplateFieldOptions
                .Include(o => o.AuditTemplateField)
                    .ThenInclude(f => f.AuditTemplateSection)
                        .ThenInclude(s => s.AuditTemplate)
                .FirstOrDefaultAsync(o => o.Id == optionId && o.AuditTemplateField.AuditTemplateSection.AuditTemplate.CompanyId == companyId);

            if (option == null)
                return false;

            _context.AuditTemplateFieldOptions.Remove(option);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReorderOptionsAsync(int templateId, int sectionId, int fieldId, List<int> optionIdsInOrder, int companyId)
        {
            var options = await _context.AuditTemplateFieldOptions
                .Include(o => o.AuditTemplateField)
                    .ThenInclude(f => f.AuditTemplateSection)
                        .ThenInclude(s => s.AuditTemplate)
                .Where(o => o.AuditTemplateFieldId == fieldId && o.AuditTemplateField.AuditTemplateSection.AuditTemplate.CompanyId == companyId)
                .ToListAsync();

            if (options == null || !options.Any())
                return false;

            for (int i = 0; i < optionIdsInOrder.Count; i++)
            {
                var option = options.FirstOrDefault(o => o.Id == optionIdsInOrder[i]);
                if (option != null)
                {
                    option.SortOrder = i;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
