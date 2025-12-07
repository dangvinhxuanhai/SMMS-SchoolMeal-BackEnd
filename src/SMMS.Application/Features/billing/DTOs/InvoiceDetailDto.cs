namespace SMMS.Application.Features.billing.DTOs;

// DTO chi tiết dùng cho trang thanh toán
public class InvoiceDetailDto
{
    // --- Thông tin cơ bản ---
    public long InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;

    // --- Thời gian & Trạng thái ---
    public short MonthNo { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int AbsentDay { get; set; }
    public string Status { get; set; } = string.Empty; // Unpaid, Paid, Cancelled

    // --- Tính toán tiền (Hiển thị minh bạch) ---
    public decimal MealPricePerDay { get; set; }
    public decimal TotalExpected { get; set; }   // Tổng gốc
    public decimal DeductedAmount { get; set; }  // Tiền trừ
    public decimal AmountToPay { get; set; }     // Tiền cần đóng

    // --- Thông tin Trường (Để hiển thị Header hóa đơn) ---
    public SchoolInfoDto? SchoolInfo { get; set; }

    // --- Lịch sử thanh toán ---
    public List<PaymentHistoryDto> PaymentHistories { get; set; } = new();
}

public class SchoolInfoDto
{
    public string SchoolName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNo { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
}

public class PaymentHistoryDto
{
    public long PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
