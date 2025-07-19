using CredWise.Models;
using CredWise.Models.ViewModels;
using CredWise.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace CredWise.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BankLoanManagementDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(BankLoanManagementDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _logger = logger;
        }


        public IActionResult CustomerDetails()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerViewModel
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    LoanApplicationCount = c.LoanApplications.Count()
                })
                .ToListAsync();

            return Json(customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerDetails(int customerId)
        {
            var customer = await _context.Customers
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.LoanApplications)
                    .ThenInclude(la => la.LoanProduct)
                .Select(c => new CustomerDetailViewModel
                {
                    CustomerId = c.CustomerId,
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address,
                    AccountNumber = c.AccountNumber,
                    CreatedDate = c.CreatedDate,
                    LoanApplications = c.LoanApplications.Select(la => new LoanApplicationSummaryViewModel
                    {
                        ApplicationId = la.ApplicationId,
                        ProductName = la.LoanProductName,
                        LoanAmount = la.LoanAmount,
                        ApplicationDate = la.ApplicationDate,
                        ApprovalStatus = la.ApprovalStatus
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found." });
            }

            return Json(customer);
        }
        public async Task<IActionResult> AdminDashboard()
        {
            var viewModel = new AdminDashboardViewModel();
            var currentYear = DateTime.Now.Year;

            // --- 1. Fetch Key Performance Indicators (KPIs) ---
            viewModel.TotalLoanValue = await _context.LoanApplications
                .Where(la => la.ApprovalStatus == LoanApprovalStatus.APPROVED.ToString() &&
                               (la.LoanStatus == LoanOverallStatus.ACTIVE.ToString() || la.LoanStatus == LoanOverallStatus.OVERDUE.ToString()))
                .SumAsync(la => la.LoanAmount);

            viewModel.ActiveLoansCount = await _context.LoanApplications
                .CountAsync(la => la.LoanStatus == LoanOverallStatus.ACTIVE.ToString());

            viewModel.PendingApplicationsCount = await _context.LoanApplications
                .CountAsync(la => la.ApprovalStatus == LoanApprovalStatus.PENDING.ToString());

            viewModel.OverdueLoansCount = await _context.LoanApplications
                .CountAsync(la => la.LoanStatus == LoanOverallStatus.OVERDUE.ToString());


            // --- 2. Fetch Data for Loan Performance Bar Chart ---
            var newLoansByMonth = await _context.LoanApplications
                .Where(la => la.ApprovalStatus == LoanApprovalStatus.APPROVED.ToString() && la.ApplicationDate.Year == currentYear)
                .GroupBy(la => la.ApplicationDate.Month)
                .Select(g => new { Month = g.Key, TotalAmount = g.Sum(la => la.LoanAmount) })
                .ToListAsync();

            var repaymentsByMonth = await _context.LoanPayments
                .Where(lp => lp.PaymentDate.HasValue && lp.PaymentDate.Value.Year == currentYear && lp.Status == "Success")
                .GroupBy(lp => lp.PaymentDate.Value.Month)
                .Select(g => new { Month = g.Key, TotalAmount = g.Sum(lp => lp.PaidAmount) })
                .ToListAsync();

            var newLoansData = new decimal[12];
            var repaymentsData = new decimal[12];
            var monthlyLabels = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12).ToArray();

            foreach (var item in newLoansByMonth)
            {
                newLoansData[item.Month - 1] = item.TotalAmount / 1000; // Convert to 'thousands' for chart readability
            }

            foreach (var item in repaymentsByMonth)
            {
                repaymentsData[item.Month - 1] = item.TotalAmount / 1000;
            }

            viewModel.MonthlyLabels = monthlyLabels.ToList();
            viewModel.NewLoansMonthlyData = newLoansData.ToList();
            viewModel.RepaymentsMonthlyData = repaymentsData.ToList();

            // --- 3. Prepare Data for Loan Status Doughnut Chart ---
            viewModel.LoanStatusLabels = new List<string> {
            "Active",
            "Pending Approval",
            "Overdue"
            };

            viewModel.LoanStatusCounts = new List<int> {
            viewModel.ActiveLoansCount,
            viewModel.PendingApplicationsCount,
            viewModel.OverdueLoansCount
            };


            // --- 4. Fetch Recent Loan Applications for Table Display ---
            viewModel.RecentLoanApplications = await _context.LoanApplications
                .Include(la => la.Customer)
                .Include(la => la.LoanProduct)
                .OrderByDescending(la => la.ApplicationDate)
                .Take(5)
                .ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> KycApproval(string status, int? pageNumber)
        {
            ViewData["CurrentFilter"] = status;
            PaginatedList<KycApproval> paginatedModel;

            try
            {
                int pageSize = 5;
                IQueryable<KycApproval> source = _context.KycApprovals
                                                         .Include(k => k.Customer)
                                                         .OrderBy(k => k.KycID);

                if (!String.IsNullOrEmpty(status) && status != "All")
                {
                    source = source.Where(k => k.Status == status);
                }

                paginatedModel = await PaginatedList<KycApproval>.CreateAsync(source, pageNumber ?? 1, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching KYC approvals for display.");
                // Gracefully handle database errors by returning an empty model to prevent view from crashing.
                var emptySource = new List<KycApproval>().AsQueryable();
                paginatedModel = await PaginatedList<KycApproval>.CreateAsync(emptySource, 1, 10);
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving KYC applications. Please try again later.");
            }

            return View(paginatedModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateKycStatus(int kycId, string status)
        {
            _logger.LogInformation($"Attempting to update KYC ID: {kycId} from frontend with status: '{status}'.");
            try
            {
                var kycApproval = await _context.KycApprovals.FindAsync(kycId);

                if (kycApproval == null)
                {
                    _logger.LogWarning($"KYC record with ID {kycId} not found for update.");
                    return Json(new { success = false, message = "KYC record not found." });
                }

                // Normalize incoming status to prevent case-sensitivity issues. E.g., "approved" -> "Approved"
                string normalizedStatus = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(status.ToLowerInvariant());
                _logger.LogInformation($"Normalized status for validation: '{normalizedStatus}'.");

                if (normalizedStatus != "Pending" && normalizedStatus != "Approved" && normalizedStatus != "Rejected")
                {
                    _logger.LogWarning($"Invalid status '{status}' (normalized to '{normalizedStatus}') provided for KYC ID {kycId}.");
                    return Json(new { success = false, message = "Invalid status provided." });
                }

                kycApproval.Status = normalizedStatus;

                if (normalizedStatus == "Approved" || normalizedStatus == "Rejected")
                {
                    kycApproval.ApprovalDate = DateTime.Now;
                }
                else
                {
                    kycApproval.ApprovalDate = null;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully updated KYC ID: {kycId} to status: {kycApproval.Status}.");

                return Json(new { success = true });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency error updating KYC ID {kycId}.");
                return Json(new { success = false, message = "Concurrency conflict. Data was modified by another user." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database update error for KYC ID {kycId}. Details: {ex.InnerException?.Message ?? ex.Message}");
                return Json(new { success = false, message = "Database error updating status. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error updating KYC ID {kycId}.");
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }
        public IActionResult GetKycDocument(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            // IMPORTANT: This path must be configured correctly for your production environment.
            // Storing documents in a folder outside the web root (wwwroot) is recommended for security.
            var documentsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "kyc_documents");
            var filePath = Path.Combine(documentsFolderPath, fileName);


            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning($"Document not found: {filePath}");
                return NotFound();
            }

            string contentType;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (extension)
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            return PhysicalFile(filePath, contentType);
        }

        public async Task<IActionResult> LoanApproval(string status = "All", int page = 1)
        {
            const int pageSize = 5;

            var query = _context.LoanApplications
                                .Include(la => la.Customer)
                                .Include(la => la.LoanProduct)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(la => la.ApprovalStatus == status);
            }

            var totalItems = await query.CountAsync();

            var loanApplications = await query
                                    .OrderByDescending(la => la.ApplicationDate)
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["CurrentFilter"] = status;

            return View(loanApplications);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLoanStatus(int loanId, string status)
        {
            var loanApplication = await _context.LoanApplications.FindAsync(loanId);

            if (loanApplication == null)
            {
                return NotFound(new { success = false, message = "Loan application not found." });
            }

            var validStatuses = new List<string> { "Approved", "Rejected", "Pending" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { success = false, message = "Invalid status provided." });
            }

            loanApplication.ApprovalStatus = status;

            if (status == "Approved")
            {
                loanApplication.ApprovalDate = DateTime.Now;
                loanApplication.LoanStatus = "Active";

                decimal principal = loanApplication.LoanAmount;
                decimal annualInterestRate = loanApplication.InterestRate;
                int tenureInMonths = loanApplication.TenureMonths;
                decimal regularEmiAmount = loanApplication.EMI;

                loanApplication.OutstandingBalance = principal;
                var approvalDate = loanApplication.ApprovalDate ?? DateTime.Now;
                loanApplication.NextDueDate = new DateTime(approvalDate.Year, approvalDate.Month, approvalDate.Day).AddMonths(1);

                // --- Generate Repayment Schedule upon Approval ---
                var existingRepayments = _context.Repayments.Where(r => r.ApplicationId == loanApplication.ApplicationId);
                _context.Repayments.RemoveRange(existingRepayments);

                List<Repayment> repayments = new List<Repayment>();
                decimal currentBalanceForSchedule = principal;
                decimal monthlyInterestRateDecimal = annualInterestRate / 12 / 100;

                for (int i = 1; i <= tenureInMonths; i++)
                {
                    DateTime dueDate = (loanApplication.NextDueDate ?? DateTime.Now.Date).AddMonths(i - 1);
                    decimal interestComponent = Math.Round(currentBalanceForSchedule * monthlyInterestRateDecimal, 2);
                    decimal principalComponent;
                    decimal actualEmiForThisMonth;

                    if (i < tenureInMonths)
                    {
                        actualEmiForThisMonth = regularEmiAmount;
                        principalComponent = actualEmiForThisMonth - interestComponent;
                        if (principalComponent < 0) principalComponent = 0;
                        if (currentBalanceForSchedule - principalComponent < 0 && (i < tenureInMonths - 1))
                        {
                            principalComponent = currentBalanceForSchedule;
                            actualEmiForThisMonth = principalComponent + interestComponent;
                        }
                    }
                    else
                    {
                        // The last EMI clears the remaining balance plus its final interest.
                        principalComponent = currentBalanceForSchedule;
                        actualEmiForThisMonth = principalComponent + interestComponent;
                    }
                    actualEmiForThisMonth = Math.Round(actualEmiForThisMonth, 2);

                    repayments.Add(new Repayment
                    {
                        ApplicationId = loanApplication.ApplicationId,
                        DueDate = dueDate,
                        AmountDue = actualEmiForThisMonth,
                        PaymentStatus = "PENDING",
                        PaymentDate = null
                    });

                    currentBalanceForSchedule -= principalComponent;
                    if (currentBalanceForSchedule < 0) currentBalanceForSchedule = 0;
                }

                if (repayments.Any())
                {
                    loanApplication.AmountDue = repayments.First().AmountDue;
                }
                else if (tenureInMonths == 0)
                {
                    loanApplication.AmountDue = principal;
                }

                await _context.Repayments.AddRangeAsync(repayments);

                if (string.IsNullOrEmpty(loanApplication.LoanNumber))
                {
                    loanApplication.LoanNumber = $"LN{DateTime.Now:yyyyMMdd}-{loanApplication.ApplicationId % 1000:D3}";
                }
            }
            else if (status == "Rejected")
            {
                loanApplication.ApprovalDate = DateTime.Now;
                loanApplication.LoanStatus = "Closed";
                loanApplication.EMI = 0;
                loanApplication.AmountDue = 0;
                loanApplication.OutstandingBalance = 0;
                loanApplication.NextDueDate = null;
                loanApplication.TenureMonths = 0;
                loanApplication.InterestRate = 0;
            }
            else // "Pending"
            {
                loanApplication.ApprovalDate = null;
                loanApplication.LoanStatus = "Pending";
                loanApplication.EMI = 0;
                loanApplication.AmountDue = 0;
                loanApplication.OutstandingBalance = 0;
                loanApplication.NextDueDate = null;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, newStatus = loanApplication.ApprovalStatus, loanId = loanApplication.ApplicationId, message = $"Loan status updated to {status}." });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency conflict for LoanId {loanId}.");
                return StatusCode(409, new { success = false, message = "Concurrency conflict: Loan was already updated. Please refresh." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating loan status for LoanId {loanId}.");
                return StatusCode(500, new { success = false, message = "An error occurred while updating the loan status." });
            }
        }

        [HttpGet]
        public IActionResult AddLoanProduct()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLoanProduct(LoanProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.LoanProducts.AnyAsync(p => p.ProductName == model.ProductName))
                {
                    ModelState.AddModelError("ProductName", "A loan product with this name already exists.");
                    return View(model);
                }

                var loanProduct = new LoanProduct
                {
                    ProductName = model.ProductName,
                    InterestRate = model.InterestRate,
                    MinAmount = model.MinAmount,
                    MaxAmount = model.MaxAmount,
                    Tenure = model.Tenure
                };

                _context.LoanProducts.Add(loanProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction("LoanProducts", "Admin");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult LoanProducts()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoanProducts()
        {
            var products = await _context.LoanProducts
                                        .Select(p => new LoanProductViewModel
                                        {
                                            ProductName = p.ProductName,
                                            InterestRate = p.InterestRate,
                                            MinAmount = p.MinAmount,
                                            MaxAmount = p.MaxAmount,
                                            Tenure = p.Tenure
                                        })
                                        .ToListAsync();
            return Json(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetLoanProductByName(string productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                return BadRequest(new { success = false, message = "Product name cannot be empty." });
            }

            var product = await _context.LoanProducts
                                        .Where(p => p.ProductName == productName)
                                        .Select(p => new LoanProductViewModel
                                        {
                                            ProductName = p.ProductName,
                                            InterestRate = p.InterestRate,
                                            MinAmount = p.MinAmount,
                                            MaxAmount = p.MaxAmount,
                                            Tenure = p.Tenure
                                        })
                                        .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { success = false, message = "Loan product not found." });
            }

            return Json(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLoanProduct([FromBody] LoanProductViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.ProductName))
            {
                return BadRequest(new { success = false, message = "Invalid product data." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { success = false, message = string.Join("; ", errors) });
            }

            var existingProduct = await _context.LoanProducts.FirstOrDefaultAsync(p => p.ProductName == model.ProductName);

            if (existingProduct == null)
            {
                return NotFound(new { success = false, message = "Loan product not found." });
            }

            existingProduct.InterestRate = model.InterestRate;
            existingProduct.MinAmount = model.MinAmount;
            existingProduct.MaxAmount = model.MaxAmount;
            existingProduct.Tenure = model.Tenure;

            try
            {
                _context.Update(existingProduct);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Loan product updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan product.");
                return StatusCode(500, new { success = false, message = $"An error occurred while updating the loan product: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLoanProduct(string productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                return BadRequest(new { success = false, message = "Product name cannot be empty." });
            }

            var loanProduct = await _context.LoanProducts.FirstOrDefaultAsync(p => p.ProductName == productName);

            if (loanProduct == null)
            {
                return NotFound(new { success = false, message = "Loan product not found." });
            }

            try
            {
                _context.LoanProducts.Remove(loanProduct);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = $"Deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan product.");
                return StatusCode(500, new { success = false, message = $"An error occurred while deleting the loan product: {ex.Message}" });
            }
        }

        public IActionResult LoanApplications()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoanApplications()
        {
            var applications = await _context.LoanApplications
                .Include(la => la.Customer)
                .Include(la => la.LoanProduct)
                .OrderByDescending(la => la.ApplicationDate)
                .Select(la => new
                {
                    la.ApplicationId,
                    la.LoanNumber,
                    CustomerName = la.Customer.Name,
                    ProductName = la.LoanProductName,
                    la.LoanAmount,
                    la.ApplicationDate,
                    la.ApprovalStatus,
                    la.LoanStatus
                })
                .ToListAsync();

            return Json(applications);
        }
    }
}