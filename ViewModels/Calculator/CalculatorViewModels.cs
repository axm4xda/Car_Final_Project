using System.ComponentModel.DataAnnotations;

namespace Car_Project.ViewModels.Calculator
{
    // Kredit kalkulyator formas? üçün ViewModel
    public class LoanCalculatorFormViewModel
    {
        [Required(ErrorMessage = "Avtomobil qiym?ti t?l?b olunur")]
        [Range(1, double.MaxValue, ErrorMessage = "Düzgün qiym?t daxil edin")]
        [Display(Name = "Car Price")]
        public decimal CarPrice { get; set; }

        [Required(ErrorMessage = "Ilkin öd?ni? t?l?b olunur")]
        [Range(0, double.MaxValue, ErrorMessage = "Düzgün m?bl?? daxil edin")]
        [Display(Name = "Down Payment")]
        public decimal DownPayment { get; set; }

        [Required(ErrorMessage = "Faiz d?r?c?si t?l?b olunur")]
        [Range(0.1, 100, ErrorMessage = "Düzgün faiz d?r?c?si daxil edin")]
        [Display(Name = "Interest Rate (%)")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Kredit müdd?ti t?l?b olunur")]
        [Display(Name = "Loan Term (months)")]
        public int LoanTermMonths { get; set; } = 60;

        public IList<int> AvailableTerms { get; set; } = new List<int> { 12, 24, 36, 48, 60, 72, 84 };
    }

    // Kalkulyator n?tic?si üçün ViewModel
    public class LoanCalculatorResultViewModel
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalInterestPayment { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal CarPrice { get; set; }
        public decimal DownPayment { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTermMonths { get; set; }
    }

    // ?sas Calculator s?hif?si ViewModel
    public class CalculatorIndexViewModel
    {
        public LoanCalculatorFormViewModel Form { get; set; } = new();
        public LoanCalculatorResultViewModel? Result { get; set; }
        public bool HasResult => Result != null;
    }
}
