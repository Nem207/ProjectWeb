# Tổng hợp thay đổi — Artist Layout & Notifications

Ngày tổng hợp: theo phiên làm việc gần nhất.
Phạm vi: chuyển Artist sang layout riêng (`_ArtistLayout`), làm đẹp trang Dashboard/Đăng bài, thêm tính năng Thông báo duyệt bài hát, bỏ ô nhập Thời lượng khỏi form.

---

## 1. `Views/Shared/_ArtistLayout.cshtml` (mới tạo / sửa)

```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Artist Panel</title>
    <link rel="stylesheet" href="~/css/artist-layout.css" />
    @await RenderSectionAsync("Styles", required: false)
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@@tabler/icons-webfont@2.44.0/tabler-icons.min.css" />
</head>
<body>
    <div class="artist-shell">
        <button class="sidebar-toggle" id="sidebarToggle" aria-label="Mở menu">
            <i class="ti ti-menu-2"></i>
        </button>

        <aside class="artist-sidebar" id="artistSidebar">
            <div class="brand">
                <i class="ti ti-brand-spotify"></i>
                <span>Artist Panel</span>
            </div>

            <nav class="nav-group">
                <div class="nav-label">Tổng quan</div>
                <a asp-controller="ArtistDashboard" asp-action="Index"
                   class="nav-item @(ViewData["ActiveMenu"]?.ToString() == "dashboard" ? "active" : "")">
                    <i class="ti ti-layout-dashboard"></i>
                    <span>Dashboard</span>
                </a>

                <div class="nav-label">Sáng tạo</div>
                <a asp-controller="ArtistDashboard" asp-action="UploadSong"
                   class="nav-item @(ViewData["ActiveMenu"]?.ToString() == "upload" ? "active" : "")">
                    <i class="ti ti-upload"></i>
                    <span>Đăng bài</span>
                </a>

                <div class="nav-label">Hệ thống</div>
                <a asp-controller="ArtistDashboard" asp-action="Notifications"
                   class="nav-item @(ViewData["ActiveMenu"]?.ToString() == "notifications" ? "active" : "")">
                    <i class="ti ti-bell"></i>
                    <span>Thông báo</span>
                </a>
            </nav>

            <div class="sidebar-footer">
                <div class="avatar">@(User.Identity?.Name?.Substring(0, 1).ToUpper() ?? "A")</div>
                <div>
                    <div class="user-name">@User.Identity?.Name</div>
                    <div class="user-role">Nghệ sĩ</div>
                </div>
            </div>
        </aside>

        <div class="artist-overlay" id="artistOverlay"></div>

        <main class="artist-main">
            <header class="artist-header">
                <div>
                    <h1>@ViewData["Title"]</h1>
                    <p>@ViewData["PageSub"]</p>
                </div>
                <form asp-controller="Auth" asp-action="Logout" method="post">
                    @Html.AntiForgeryToken()
                    <button type="submit" class="btn-logout">Đăng xuất</button>
                </form>
            </header>

            <section class="artist-content">
                @RenderBody()
            </section>
        </main>
    </div>

    <script src="~/js/artist-layout.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

## 2. `Views/ArtistDashboard/_ViewStart.cshtml` (mới tạo)

```csharp
@{
    Layout = "_ArtistLayout";
}
```

Mục đích: mọi View trong `Views/ArtistDashboard/` tự động dùng `_ArtistLayout`, khắc phục lỗi bị dính layout User trước đó.

---

## 3. `Views/ArtistDashboard/Index.cshtml` (sửa)

@model SpotifyClone.Features.ArtistDashboard.ViewModels.ArtistDashboardViewModel
@{
    ViewData["Title"] = "Dashboard";
    ViewData["ActiveMenu"] = "dashboard";
}

<div class="profile-header">
    @if (!string.IsNullOrEmpty(Model.Avatar))
    {
        <img src="@Model.Avatar" class="profile-avatar" alt="avatar" />
    }
    else
    {
        <div class="profile-avatar" style="display:flex;align-items:center;justify-content:center;background:var(--bg-card);font-weight:700;font-size:28px;">
            @Model.ArtistName.Substring(0, 1).ToUpper()
        </div>
    }
    <div>
        <h1 class="profile-name">@Model.ArtistName</h1>
        <a asp-action="UploadSong" class="btn-primary">
            <i class="ti ti-plus"></i> Đăng bài hát mới
        </a>
    </div>
</div>

<div class="stat-grid">
    <div class="stat-card">
        <div class="stat-value">@Model.TotalSongs</div>
        <div class="stat-label">Bài hát</div>
    </div>
    <div class="stat-card">
        <div class="stat-value">@Model.TotalPlays.ToString("N0")</div>
        <div class="stat-label">Lượt nghe</div>
    </div>
    <div class="stat-card">
        <div class="stat-value">@Model.TotalLikes.ToString("N0")</div>
        <div class="stat-label">Lượt thích</div>
    </div>
    <div class="stat-card">
        <div class="stat-value">@Model.MonthlyListeners.ToString("N0")</div>
        <div class="stat-label">Người nghe hàng tháng</div>
    </div>
    <div class="stat-card">
        <div class="stat-value">@Model.TotalFollowers.ToString("N0")</div>
        <div class="stat-label">Người theo dõi</div>
    </div>
</div>

<div class="card">
    <h2 class="card-title">Top bài hát</h2>

    @if (Model.TopSongs != null && Model.TopSongs.Any())
    {
        <table class="data-table">
            <thead>
                <tr>
                    <th>Bài hát</th>
                    <th>Trạng thái</th>
                    <th>Lượt nghe</th>
                    <th>Lượt thích</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var song in Model.TopSongs)
                {
                    <tr>
                        <td style="display:flex;align-items:center;gap:10px;">
                            @if (!string.IsNullOrEmpty(song.CoverImage))
                            {
                                <img src="@song.CoverImage" style="width:36px;height:36px;border-radius:6px;object-fit:cover;" />
                            }
                            <span>@song.Title</span>
                        </td>
                        <td>
                            @{
                                var badgeClass = song.Status switch
                                {
                                    "Approved" => "badge-approved",
                                    "Rejected" => "badge-rejected",
                                    _ => "badge-pending"
                                };
                            }
                            <span class="badge @badgeClass">@song.Status</span>
                        </td>
                        <td>@song.TotalPlays.ToString("N0")</td>
                        <td>@song.TotalLikes.ToString("N0")</td>
                    </tr>
                }
            </tbody>
        </table>
    }
    else
    {
        <div class="empty-state">Bạn chưa có bài hát nào.</div>
    }
</div>

## 4. `Views/ArtistDashboard/UploadSong.cshtml` (sửa — đã bỏ ô Thời lượng và ô Premium)

@model SpotifyClone.Features.ArtistDashboard.ViewModels.UploadSongViewModel
@{
    ViewData["Title"] = "Đăng bài hát";
    ViewData["ActiveMenu"] = "upload";
}

<div class="card form-card">
    <h2 class="card-title">Đăng bài hát mới</h2>
    <p style="color:var(--text-muted); font-size:13px; margin-top:-8px; margin-bottom:20px;">
        Bài hát sau khi gửi sẽ ở trạng thái <strong style="color:#ffc107;">Pending</strong> — chờ Admin duyệt trước khi hiển thị công khai.
    </p>

    <form asp-action="UploadSong" method="post">
        @Html.AntiForgeryToken()
        <div asp-validation-summary="ModelOnly" class="text-danger"></div>

        <input asp-for="Duration" type="hidden" value="1" />

        <div class="form-group">
            <label asp-for="Title">Tên bài hát</label>
            <input asp-for="Title" class="form-control" placeholder="Nhập tên bài hát" />
            <span asp-validation-for="Title" class="text-danger"></span>
        </div>

        <div class="form-group">
            <label asp-for="AudioURL">Link file nhạc (URL)</label>
            <input asp-for="AudioURL" class="form-control" placeholder="https://..." />
            <span asp-validation-for="AudioURL" class="text-danger"></span>
        </div>

        <div class="form-group">
            <label asp-for="CoverImage">Link ảnh bìa (URL, tuỳ chọn)</label>
            <input asp-for="CoverImage" class="form-control" placeholder="https://..." />
            <span asp-validation-for="CoverImage" class="text-danger"></span>
        </div>

        <div class="form-actions">
            <button type="submit" class="btn-primary">Gửi bài hát</button>
            <a asp-action="Index" class="btn-cancel">Huỷ</a>
        </div>
    </form>
</div>

@section Scripts {
    @{
        await Html.RenderPartialAsync("_ValidationScriptsPartial");
    }
}

## 5. `Views/ArtistDashboard/Notifications.cshtml` (mới tạo)

@model List<SpotifyClone.Features.ArtistDashboard.ViewModels.ArtistNotificationItem>
@{
    ViewData["Title"] = "Thông báo";
    ViewData["ActiveMenu"] = "notifications";
}

@section Styles {
    <link rel="stylesheet" href="~/css/artist-notifications.css" />
}

<div class="card">
    <h2 class="card-title">Thông báo duyệt bài hát</h2>

    @if (Model != null && Model.Any())
    {
        foreach (var item in Model)
        {
            var isApproved = item.Status == "Approved";
            <div class="notif-item">
                <div class="notif-icon @(isApproved ? "approved" : "rejected")">
                    <i class="ti @(isApproved ? "ti-check" : "ti-x")"></i>
                </div>
                <div>
                    <div class="notif-text">
                        Bài hát <strong>"@item.Title"</strong> đã được
                        @if (isApproved)
                        {
                            <span style="color:var(--accent);font-weight:600;">duyệt</span>
                        }
                        else
                        {
                            <span style="color:#dc3545;font-weight:600;">từ chối</span>
                        }
                    </div>
                    @if (!isApproved && !string.IsNullOrEmpty(item.RejectReason))
                    {
                        <div class="notif-text" style="color:var(--text-muted);margin-top:2px;">
                            Lý do: @item.RejectReason
                        </div>
                    }
                    <div class="notif-time">@item.CreatedAt.ToString("dd/MM/yyyy HH:mm")</div>
                </div>
            </div>
        }
    }
    else
    {
        <div class="empty-state">Chưa có thông báo nào.</div>
    }
</div>

## 6. `ViewModels/ArtistDashboardViewModel.cs` (sửa — bỏ Required/Range khỏi Duration)

using System.ComponentModel.DataAnnotations;

namespace SpotifyClone.Features.ArtistDashboard.ViewModels;

public class ArtistDashboardViewModel
{
    public int ArtistID { get; set; }
    public string ArtistName { get; set; } = "";
    public string? Avatar { get; set; }

    public int TotalSongs { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public long MonthlyListeners { get; set; }
    public long TotalFollowers { get; set; }

    public List<ArtistTopSong> TopSongs { get; set; } = new();
}

public class ArtistTopSong
{
    public int SongId { get; set; }
    public string Title { get; set; } = "";
    public string? CoverImage { get; set; }
    public long TotalPlays { get; set; }
    public long TotalLikes { get; set; }
    public string Status { get; set; } = "";
}

public class UploadSongViewModel
{
    [Required(ErrorMessage = "Vui long nhap ten bai hat")]
    [StringLength(255)]
    public string Title { get; set; } = "";

    // Khong bat buoc nhap tu form nua; mac dinh = 1, se duoc cap nhat chinh xac sau
    // (VD: doc metadata tu file audio, hoac Admin cap nhat thu cong khi duyet).
    public int Duration { get; set; } = 1;

    [Required(ErrorMessage = "Vui long nhap URL file nhac")]
    [Url(ErrorMessage = "AudioURL khong hop le")]
    public string AudioURL { get; set; } = "";

    [Url(ErrorMessage = "CoverImage khong hop le")]
    public string? CoverImage { get; set; }

    public bool IsPremium { get; set; }
}

public class ArtistNotificationItem
{
    public int SongId { get; set; }
    public string Title { get; set; } = "";
    public string? CoverImage { get; set; }
    public string Status { get; set; } = "";
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

## 7. `Services/ArtistDashboardService.cs` (sửa — thêm method Notifications)

using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Features.ArtistDashboard.ViewModels;

namespace SpotifyClone.Features.ArtistDashboard.Services;

public interface IArtistDashboardService
{
    /// <summary>
    /// Lay du lieu dashboard cho ho so nghe si gan voi userId dang dang nhap.
    /// Tra ve null neu user nay chua duoc gan (Artists.UserID) voi ho so nghe si nao.
    /// </summary>
    Task<ArtistDashboardViewModel?> GetDashboardForUserAsync(int userId);

    /// <summary>
    /// Tao 1 bai hat moi cho nghe si gan voi userId, Status mac dinh = Pending cho toi khi Admin duyet.
    /// Tra ve false neu userId khong lien ket voi ho so nghe si nao (Artists.UserID).
    /// </summary>
    Task<bool> CreateSongAsync(int userId, UploadSongViewModel input);

    /// <summary>
    /// Lay danh sach bai hat da duoc Admin xu ly (Approved/Rejected) cua nghe si gan voi userId,
    /// sap xep moi nhat len dau. Tra ve null neu userId chua lien ket ho so nghe si nao.
    /// </summary>
    Task<List<ArtistNotificationItem>?> GetNotificationsForUserAsync(int userId);
}

public class ArtistDashboardService : IArtistDashboardService
{
    private readonly SpotifyDbContext _context;

    public ArtistDashboardService(SpotifyDbContext context)
    {
        _context = context;
    }

    public async Task<ArtistDashboardViewModel?> GetDashboardForUserAsync(int userId)
    {
        var artist = await _context.Artists
            .Include(a => a.ArtistStatistic)
            .FirstOrDefaultAsync(a => a.UserID == userId);

        if (artist == null)
        {
            return null;
        }

        var songIds = await _context.SongArtists
            .Where(sa => sa.ArtistID == artist.ArtistID)
            .Select(sa => sa.SongID)
            .ToListAsync();

        var songs = await _context.Songs
            .Where(s => songIds.Contains(s.SongID))
            .ToListAsync();

        var stats = await _context.SongStatistics
            .Where(s => songIds.Contains(s.SongID))
            .ToListAsync();

        var topSongs = songs
            .Select(s =>
            {
                var stat = stats.FirstOrDefault(x => x.SongID == s.SongID);
                return new ArtistTopSong
                {
                    SongId = s.SongID,
                    Title = s.Title,
                    CoverImage = s.CoverImage,
                    Status = s.Status,
                    TotalPlays = stat?.TotalPlays ?? 0,
                    TotalLikes = stat?.TotalLikes ?? 0
                };
            })
            .OrderByDescending(s => s.TotalPlays)
            .Take(10)
            .ToList();

        return new ArtistDashboardViewModel
        {
            ArtistID = artist.ArtistID,
            ArtistName = artist.ArtistName ?? "",
            Avatar = artist.Avatar,
            TotalSongs = songs.Count,
            TotalPlays = stats.Sum(s => s.TotalPlays),
            TotalLikes = stats.Sum(s => s.TotalLikes),
            MonthlyListeners = artist.ArtistStatistic?.MonthlyListeners ?? 0,
            TotalFollowers = artist.ArtistStatistic?.TotalFollowers ?? 0,
            TopSongs = topSongs
        };
    }

    public async Task<bool> CreateSongAsync(int userId, UploadSongViewModel input)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
        if (artist == null)
        {
            return false;
        }

        var song = new Models.Song
        {
            Title = input.Title,
            Duration = input.Duration,
            AudioURL = input.AudioURL,
            CoverImage = input.CoverImage,
            IsPremium = input.IsPremium,
            ReleaseDate = DateTime.Now,
            CreatedAt = DateTime.Now,
            Status = Models.SongStatus.Pending // Cho Admin duyet, khong tu dong Approved
        };

        _context.Songs.Add(song);
        await _context.SaveChangesAsync(); // can SongID truoc khi tao lien ket SongArtist

        _context.SongArtists.Add(new Models.SongArtist
        {
            SongID = song.SongID,
            ArtistID = artist.ArtistID
        });

        // Khoi tao thong ke rong cho bai hat moi de cac truy van thong ke (Dashboard) khong bi thieu dong
        _context.SongStatistics.Add(new Models.SongStatistic
        {
            SongID = song.SongID,
            TotalPlays = 0,
            TotalLikes = 0,
            TotalDownloads = 0
        });

        await _context.SaveChangesAsync();
        return true;
    }

    // ==== MỚI THÊM ====
    public async Task<List<ArtistNotificationItem>?> GetNotificationsForUserAsync(int userId)
    {
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.UserID == userId);
        if (artist == null)
        {
            return null;
        }

        var songIds = await _context.SongArtists
            .Where(sa => sa.ArtistID == artist.ArtistID)
            .Select(sa => sa.SongID)
            .ToListAsync();

        var notifications = await _context.Songs
            .Where(s => songIds.Contains(s.SongID) &&
                        (s.Status == Models.SongStatus.Approved || s.Status == Models.SongStatus.Rejected))
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ArtistNotificationItem
            {
                SongId = s.SongID,
                Title = s.Title,
                CoverImage = s.CoverImage,
                Status = s.Status,
                RejectReason = s.RejectReason,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return notifications;
    }
}
## 8. `Controllers/ArtistDashboardController.cs` (sửa — thêm action Notifications)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyClone.Features.ArtistDashboard.Services;
using SpotifyClone.Features.Auth.Services;

namespace SpotifyClone.Features.ArtistDashboard.Controllers;

// Khu vuc rieng cho tai khoan co Role = "Artist".
// Chi hien thi du lieu (bai hat, luot nghe, luot thich...) cua CHINH nghe si dang dang nhap,
// duoc xac dinh qua lien ket Artists.UserID <-> Users.UserID.
[Authorize(Roles = "Artist")]
public class ArtistDashboardController : Controller
{
    private readonly IArtistDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;

    public ArtistDashboardController(
        IArtistDashboardService dashboardService,
        ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    // GET: /ArtistDashboard
    public async Task<IActionResult> Index()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
        {
            return Forbid();
        }

        var model = await _dashboardService.GetDashboardForUserAsync(userId.Value);
        if (model == null)
        {
            // Tai khoan co Role="Artist" nhung chua duoc gan (Artists.UserID) voi ho so nghe si nao.
            // Can Admin gan thu cong (xem add_artist_userid.sql) truoc khi dashboard co du lieu de hien thi.
            return View("NotLinked");
        }

        return View(model);
    }

    // GET: /ArtistDashboard/UploadSong
    public async Task<IActionResult> UploadSong()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();

        var isLinked = await _dashboardService.GetDashboardForUserAsync(userId.Value) != null;
        if (!isLinked)
        {
            return View("NotLinked");
        }

        return View(new SpotifyClone.Features.ArtistDashboard.ViewModels.UploadSongViewModel());
    }

    // POST: /ArtistDashboard/UploadSong
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadSong(SpotifyClone.Features.ArtistDashboard.ViewModels.UploadSongViewModel input)
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();

        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var success = await _dashboardService.CreateSongAsync(userId.Value, input);
        if (!success)
        {
            // Truong hop hiem: Role=Artist nhung bi go lien ket Artists.UserID giua chung
            return View("NotLinked");
        }

        TempData["SuccessMessage"] = "Da gui bai hat, cho Admin duyet.";
        return RedirectToAction(nameof(Index));
    }

    // ==== MỚI THÊM ====
    // GET: /ArtistDashboard/Notifications
    public async Task<IActionResult> Notifications()
    {
        var userId = _currentUser.UserId;
        if (userId == null) return Forbid();

        var model = await _dashboardService.GetNotificationsForUserAsync(userId.Value);
        if (model == null)
        {
            return View("NotLinked");
        }

        return View(model);
    }
}
## 9. `wwwroot/css/artist-layout.css` (sửa — thêm style card/table/form/stat)

Thêm vào cuối file (giữ nguyên phần gốc bạn đã có: biến root, sidebar, main, responsive):

```css
:root {
    --bg-main: #0d0d0d;
    --bg-panel: #121212;
    --bg-card: #181818;
    --accent: #1db954;
    --text-main: #ffffff;
    --text-muted: #a0a0a0;
    --border-color: #2a2a2a;
    --sidebar-width: 260px;
}

* {
    box-sizing: border-box;
}

body {
    margin: 0;
    background: var(--bg-main);
    color: var(--text-main);
    font-family: "Segoe UI", Roboto, sans-serif;
}

.artist-shell {
    display: flex;
    min-height: 100vh;
}

/* Sidebar */
.artist-sidebar {
    width: var(--sidebar-width);
    background: var(--bg-panel);
    border-right: 1px solid var(--border-color);
    display: flex;
    flex-direction: column;
    padding: 20px 16px;
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    z-index: 100;
    transition: transform 0.25s ease;
}

.brand {
    display: flex;
    align-items: center;
    gap: 10px;
    font-weight: 700;
    font-size: 18px;
    padding: 8px 8px 24px;
}

    .brand i {
        color: var(--accent);
        font-size: 26px;
    }

.nav-label {
    font-size: 11px;
    text-transform: uppercase;
    color: var(--text-muted);
    letter-spacing: 0.06em;
    margin: 12px 8px 8px;
}

.nav-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 12px;
    border-radius: 8px;
    color: var(--text-main);
    text-decoration: none;
    font-size: 14px;
    margin-bottom: 4px;
}

    .nav-item i {
        font-size: 18px;
        color: var(--text-muted);
    }

    .nav-item:hover {
        background: rgba(255, 255, 255, 0.05);
    }

    .nav-item.active {
        background: rgba(29, 185, 84, 0.15);
        color: var(--accent);
        border-left: 3px solid var(--accent);
    }

        .nav-item.active i {
            color: var(--accent);
        }

.sidebar-footer {
    margin-top: auto;
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 8px;
    border-top: 1px solid var(--border-color);
}

.avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    background: var(--accent);
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
    color: #000;
    flex-shrink: 0;
}

.user-name {
    font-size: 13px;
    font-weight: 600;
}

.user-role {
    font-size: 11px;
    color: var(--text-muted);
}

/* Main content */
.artist-main {
    margin-left: var(--sidebar-width);
    flex: 1;
    padding: 28px 32px;
    width: 100%;
}

.artist-header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 24px;
    gap: 16px;
    flex-wrap: wrap;
}

    .artist-header h1 {
        margin: 0 0 4px;
        font-size: 26px;
    }

    .artist-header p {
        margin: 0;
        color: var(--text-muted);
        font-size: 14px;
    }

.btn-logout {
    background: transparent;
    border: 1px solid var(--border-color);
    color: var(--text-main);
    padding: 8px 16px;
    border-radius: 8px;
    cursor: pointer;
    font-size: 13px;
    white-space: nowrap;
}

    .btn-logout:hover {
        border-color: var(--accent);
        color: var(--accent);
    }

/* Sidebar toggle (mobile) */
.sidebar-toggle {
    display: none;
    position: fixed;
    top: 14px;
    left: 14px;
    z-index: 200;
    background: var(--bg-card);
    border: 1px solid var(--border-color);
    color: var(--text-main);
    width: 40px;
    height: 40px;
    border-radius: 8px;
    font-size: 18px;
    cursor: pointer;
}

.artist-overlay {
    display: none;
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    z-index: 99;
}

/* ===== RESPONSIVE ===== */
@media (max-width: 900px) {
    .artist-sidebar {
        transform: translateX(-100%);
    }

        .artist-sidebar.open {
            transform: translateX(0);
        }

    .artist-main {
        margin-left: 0;
        padding: 80px 20px 32px;
    }

    .sidebar-toggle {
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .artist-overlay.show {
        display: block;
    }
}

@media (max-width: 480px) {
    .artist-main {
        padding: 76px 14px 24px;
    }

    .artist-header h1 {
        font-size: 21px;
    }
}

.nav-item.disabled {
    opacity: 0.4;
    cursor: not-allowed;
    pointer-events: none;
}

/* ===== Profile header ===== */
.profile-header {
    display: flex;
    align-items: center;
    gap: 20px;
    margin-bottom: 28px;
}

.profile-avatar {
    width: 88px;
    height: 88px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid var(--border-color);
}

.profile-name {
    font-size: 28px;
    font-weight: 700;
    margin: 0 0 8px;
}

.btn-primary {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    background: var(--accent);
    color: #000;
    font-weight: 600;
    padding: 10px 18px;
    border-radius: 24px;
    border: none;
    text-decoration: none;
    font-size: 14px;
    cursor: pointer;
}

    .btn-primary:hover {
        background: #1ed760;
        color: #000;
    }

/* ===== Stat cards ===== */
.stat-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
    gap: 16px;
    margin: 24px 0 32px;
}

.stat-card {
    background: var(--bg-card);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 18px 20px;
}

.stat-value {
    font-size: 26px;
    font-weight: 700;
    color: var(--accent);
}

.stat-label {
    font-size: 13px;
    color: var(--text-muted);
    margin-top: 4px;
}

/* ===== Section card / table ===== */
.card {
    background: var(--bg-card);
    border: 1px solid var(--border-color);
    border-radius: 12px;
    padding: 20px 24px;
    margin-bottom: 24px;
}

.card-title {
    font-size: 16px;
    font-weight: 700;
    margin: 0 0 16px;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
}

    .data-table th {
        text-align: left;
        color: var(--text-muted);
        font-weight: 600;
        font-size: 12px;
        text-transform: uppercase;
        padding: 10px 12px;
        border-bottom: 1px solid var(--border-color);
    }

    .data-table td {
        padding: 12px;
        border-bottom: 1px solid var(--border-color);
    }

    .data-table tr:last-child td {
        border-bottom: none;
    }

.badge {
    display: inline-block;
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 600;
}

.badge-approved {
    background: rgba(29,185,84,0.15);
    color: var(--accent);
}

.badge-pending {
    background: rgba(255,193,7,0.15);
    color: #ffc107;
}

.badge-rejected {
    background: rgba(220,53,69,0.15);
    color: #dc3545;
}

/* ===== Form ===== */
.form-card {
    max-width: 560px;
}

.form-group {
    margin-bottom: 18px;
}

    .form-group label {
        display: block;
        font-size: 13px;
        font-weight: 600;
        margin-bottom: 6px;
        color: var(--text-muted);
    }

.form-control {
    width: 100%;
    background: var(--bg-main);
    border: 1px solid var(--border-color);
    border-radius: 8px;
    padding: 10px 14px;
    color: var(--text-main);
    font-size: 14px;
}

    .form-control:focus {
        outline: none;
        border-color: var(--accent);
    }

.form-check {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 20px;
    font-size: 14px;
}

.form-actions {
    display: flex;
    align-items: center;
    gap: 16px;
}

.btn-cancel {
    color: var(--text-muted);
    text-decoration: none;
    font-size: 14px;
}

    .btn-cancel:hover {
        color: var(--text-main);
    }

.text-danger {
    color: #dc3545;
    font-size: 12px;
    margin-top: 4px;
    display: block;
}

/* ===== Empty state ===== */
.empty-state {
    text-align: center;
    padding: 40px 20px;
    color: var(--text-muted);
}

/* ===== Notification list ===== */
.notif-item {
    display: flex;
    gap: 14px;
    padding: 14px 12px;
    border-bottom: 1px solid var(--border-color);
}

    .notif-item:last-child {
        border-bottom: none;
    }

.notif-icon {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-size: 16px;
}

    .notif-icon.approved {
        background: rgba(29,185,84,0.15);
        color: var(--accent);
    }

    .notif-icon.rejected {
        background: rgba(220,53,69,0.15);
        color: #dc3545;
    }

.notif-text {
    font-size: 14px;
}

.notif-time {
    font-size: 12px;
    color: var(--text-muted);
    margin-top: 2px;
}


## 10. `wwwroot/css/artist-notifications.css` (sửa — thêm style trang Thông báo)

.notif-item {
    display: flex;
    gap: 14px;
    padding: 14px 12px;
    border-bottom: 1px solid var(--border-color);
}

    .notif-item:last-child {
        border-bottom: none;
    }

.notif-icon {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-size: 16px;
}

    .notif-icon.approved {
        background: rgba(29,185,84,0.15);
        color: var(--accent);
    }

    .notif-icon.rejected {
        background: rgba(220,53,69,0.15);
        color: #dc3545;
    }

.notif-text {
    font-size: 14px;
}

.notif-time {
    font-size: 12px;
    color: var(--text-muted);
    margin-top: 2px;
}


## 11. `wwwroot/js/artist-layout.js` — không đổi

document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("sidebarToggle");
    const sidebar = document.getElementById("artistSidebar");
    const overlay = document.getElementById("artistOverlay");

    function openSidebar() {
        sidebar.classList.add("open");
        overlay.classList.add("show");
    }

    function closeSidebar() {
        sidebar.classList.remove("open");
        overlay.classList.remove("show");
    }

    toggleBtn?.addEventListener("click", function () {
        sidebar.classList.contains("open") ? closeSidebar() : openSidebar();
    });

    overlay?.addEventListener("click", closeSidebar);

    // Đóng sidebar khi bấm chọn 1 menu (trên mobile)
    document.querySelectorAll(".nav-item").forEach(function (link) {
        link.addEventListener("click", closeSidebar);
    });
});
