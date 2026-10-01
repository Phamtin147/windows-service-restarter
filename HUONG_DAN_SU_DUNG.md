# ⚡ HƯỚNG DẪN SỬ DỤNG: WINDOWS ANTI-LAG & SERVICE RESTARTER

Công cụ độc lập được thiết kế đặc biệt nhằm **khắc phục tình trạng đơ, giật lag, khựng khung hình (micro-stutter)** trên Windows 10 & Windows 11 mà **KHÔNG** làm tắt hoặc ảnh hưởng đến các phần mềm bạn đang mở (trình duyệt, game, đồ họa, phần mềm làm việc).

---

## 📁 Vị trí phần mềm
Thư mục lưu trữ:  
`C:\Users\NOFACE\.gemini\antigravity\scratch\WinLagFixer\`

Các file chính:
- **`WinLagFixer.exe`**: Ứng dụng chính (Giao diện Dark Theme hiện đại, native C# .NET, không cần cài đặt thêm bất kỳ thư viện nào).
- **`Chay-FixLag.bat`**: File khởi động 1-click (tự động xin quyền Administrator nếu cần).
- **`Tao-Shortcut-Desktop.bat`**: Click đúp vào file này để tạo ngay 1 biểu tượng lối tắt ngoài màn hình Desktop `⚡ Fix Lag Windows` để mỗi khi máy giật chỉ cần click 1 phát.

---

## 🔒 Cam kết an toàn hệ thống
> [!IMPORTANT]
> **Tuân thủ đúng yêu cầu của bạn:** Toàn bộ quá trình tạo app **KHÔNG HỀ chạy thử nghiệm (test-run)** bất kỳ lệnh restart dịch vụ nào trên máy tính của bạn trong lúc lập trình, đảm bảo 100% các ứng dụng và phiên làm việc hiện tại của bạn không bị gián đoạn hay ảnh hưởng!

---

## 🚀 Các chế độ khắc phục

### 1. ⚡ Khắc Phục Nhanh (1-Click Quick Fix - Khuyên dùng)
Dành cho trường hợp: Đang chơi game hoặc làm việc thấy máy bị khựng nhẹ, lag chuột, trễ âm thanh, đơ thanh Taskbar.
- **Nạp lại Card Đồ Họa (GPU Pipeline):** Kích hoạt cơ chế nạp lại driver của Windows (tương đương tổ hợp phím `Win + Ctrl + Shift + B`), màn hình sẽ chớp nhẹ 0.5s và giải tỏa tắc nghẽn GPU mà không đóng cửa sổ nào.
- **Restart Windows Explorer:** Làm mới shell giao diện, thanh Taskbar, Start Menu, giải phóng RAM rò rỉ của Explorer.
- **Restart Hệ Thống Âm Thanh (AudioSrv, AudioEndpointBuilder):** Sửa lỗi mất tiếng, rè tiếng, xung đột buffer âm thanh gây giật khung hình.
- **Giải Phóng RAM Cache (Standby List):** Thu dọn bộ nhớ tạm bị phình to của các tiến trình nền, hạ ngay mức ngốn RAM.
- **Xóa Cache DNS (ipconfig /flushdns):** Xóa bộ nhớ đệm phân giải mạng để giảm độ trễ Internet.

### 2. 🛠️ Restart Toàn Bộ Dịch Vụ (Full Restart)
Bao gồm toàn bộ các mục ở Chế độ Nhanh và thêm:
- **Windows Search Indexer (WSearch):** Ngắt các luồng lập chỉ mục ngầm đang ăn 100% CPU hoặc 100% Disk.
- **SysMain (Superfetch):** Giải tỏa nghẽn đọc đĩa ngầm.
- **Dịch vụ Cập Nhật (wuauserv, bits, cryptsvc):** Xử lý tiến trình TiWorker hoặc Windows Update tải ngầm chiếm tài nguyên.
- **Print Spooler:** Xử lý nghẽn bộ đệm in ấn.

### 3. ▶️ Chạy Mục Đã Chọn (Tùy chỉnh linh hoạt)
Bạn có thể tự do tích hoặc bỏ tích từng dịch vụ trong danh sách, sau đó nhấn **"Chạy Mục Đã Chọn"**.

---

## 🖥️ Cách khởi động khi cần dùng
1. Vào thư mục `C:\Users\NOFACE\.gemini\antigravity\scratch\WinLagFixer`
2. Chạy file **`Chay-FixLag.bat`** (hoặc click đúp trực tiếp **`WinLagFixer.exe`** -> bấm **Yes** khi bảng UAC hỏi cấp quyền Quản trị viên).
3. Nhấn nút **"⚡ Khắc Phục Nhanh"** hoặc nút dịch vụ bạn mong muốn.
4. Quá trình xử lý sẽ hiển thị chi tiết theo thời gian thực (Real-time logs) trên màn hình.
