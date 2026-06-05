using Microsoft.EntityFrameworkCore;
using SmartAuditX.Data;
using SmartAuditX.Models.ViewModels;
using SmartAuditX.Services.Interfaces;

namespace SmartAuditX.Services.Implementations
{
    public class BranchDepartmentService : IBranchDepartmentService
    {
        private readonly ApplicationDbContext _context;

        public BranchDepartmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<BranchDepartmentListItemViewModel>> GetAllAsync(
            int companyId,
            int? branchId = null,
            int? departmentId = null)
        {
            var query = _context.BranchDepartments
                .Where(link => link.Branch.CompanyId == companyId);

            if (branchId.HasValue)
            {
                query = query.Where(link => link.BranchId == branchId.Value);
            }

            if (departmentId.HasValue)
            {
                query = query.Where(link => link.DepartmentId == departmentId.Value);
            }

            return await query
                .OrderBy(link => link.Branch.BranchName)
                .ThenBy(link => link.Department.Name)
                .Select(link => new BranchDepartmentListItemViewModel
                {
                    BranchDepartmentId = link.BranchDepartmentId,
                    BranchId = link.BranchId,
                    BranchName = link.Branch.BranchName,
                    BranchCode = link.Branch.BranchCode,
                    DepartmentId = link.DepartmentId,
                    DepartmentName = link.Department.Name,
                    DepartmentCode = link.Department.Code,
                    CreatedAt = link.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<BranchListItemViewModel>> GetBranchesForDropdownAsync(int companyId)
        {
            return await _context.Branches
                .Where(branch =>
                    branch.CompanyId == companyId &&
                    branch.IsActive &&
                    !branch.IsDeleted)
                .OrderBy(branch => branch.BranchName)
                .Select(branch => new BranchListItemViewModel
                {
                    BranchId = branch.BranchId,
                    BranchName = branch.BranchName,
                    BranchCode = branch.BranchCode,
                    IsActive = branch.IsActive,
                    CreatedAt = branch.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<DepartmentListItemViewModel>> GetDepartmentsForDropdownAsync(int companyId)
        {
            return await _context.Departments
                .Where(department =>
                    department.CompanyId == companyId &&
                    department.IsActive &&
                    !department.IsDeleted)
                .OrderBy(department => department.Name)
                .Select(department => new DepartmentListItemViewModel
                {
                    DepartmentId = department.DepartmentId,
                    Name = department.Name,
                    Code = department.Code,
                    IsActive = department.IsActive,
                    CreatedAt = department.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<BranchDepartmentOperationResult> CreateAsync(
            int companyId,
            BranchDepartmentViewModel model)
        {
            var branchExists = await _context.Branches.AnyAsync(branch =>
                branch.BranchId == model.BranchId &&
                branch.CompanyId == companyId &&
                !branch.IsDeleted);

            if (!branchExists)
            {
                return BranchDepartmentOperationResult.Fail("Selected branch is invalid or not found.");
            }

            var departmentExists = await _context.Departments.AnyAsync(department =>
                department.DepartmentId == model.DepartmentId &&
                department.CompanyId == companyId &&
                !department.IsDeleted);

            if (!departmentExists)
            {
                return BranchDepartmentOperationResult.Fail("Selected department is invalid or not found.");
            }

            var linkExists = await _context.BranchDepartments.AnyAsync(link =>
                link.BranchId == model.BranchId &&
                link.DepartmentId == model.DepartmentId);

            if (linkExists)
            {
                return BranchDepartmentOperationResult.Fail(
                    "This department is already linked to the selected branch.");
            }

            var link = new Models.BranchDepartment
            {
                BranchId = model.BranchId,
                DepartmentId = model.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.BranchDepartments.AddAsync(link);
            await _context.SaveChangesAsync();

            return BranchDepartmentOperationResult.Ok(
                "Department linked to branch successfully.",
                await MapToListItemAsync(link));
        }

        public async Task<BranchDepartmentOperationResult> DeleteAsync(
            int companyId,
            int branchDepartmentId)
        {
            var link = await _context.BranchDepartments
                .FirstOrDefaultAsync(item =>
                    item.BranchDepartmentId == branchDepartmentId &&
                    item.Branch.CompanyId == companyId);

            if (link == null)
            {
                return BranchDepartmentOperationResult.Fail("Link not found.");
            }

            _context.BranchDepartments.Remove(link);
            await _context.SaveChangesAsync();

            return BranchDepartmentOperationResult.Ok("Department unlinked from branch successfully.");
        }

        private async Task<BranchDepartmentListItemViewModel> MapToListItemAsync(
            Models.BranchDepartment link)
        {
            var branch = await _context.Branches
                .FirstAsync(b => b.BranchId == link.BranchId);

            var department = await _context.Departments
                .FirstAsync(d => d.DepartmentId == link.DepartmentId);

            return new BranchDepartmentListItemViewModel
            {
                BranchDepartmentId = link.BranchDepartmentId,
                BranchId = link.BranchId,
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                DepartmentId = link.DepartmentId,
                DepartmentName = department.Name,
                DepartmentCode = department.Code,
                CreatedAt = link.CreatedAt
            };
        }
    }
}
