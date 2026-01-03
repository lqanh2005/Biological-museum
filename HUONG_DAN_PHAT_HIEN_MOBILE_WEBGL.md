# Hướng Dẫn Phát Hiện Mobile Device trong WebGL

## 📋 Tổng Quan

Trong WebGL, Unity không thể tự động phát hiện mobile device như trên native platform. Giải pháp này sử dụng **JavaScript** để phát hiện chính xác thiết bị mobile.

## 🔧 Cách Hoạt Động

### 1. **MobileDetector.cs** (C# Script)
- Script Unity để gọi JavaScript function
- Cache kết quả để tối ưu performance
- Tự động tạo instance khi cần

### 2. **MobileDetection.jslib** (JavaScript Library)
- File JavaScript được Unity tự động include khi build WebGL
- Kiểm tra:
  - **User Agent**: Tìm các từ khóa như "android", "iphone", "ipad", etc.
  - **Touch Support**: Kiểm tra xem trình duyệt có hỗ trợ touch không
  - **Screen Size**: Màn hình nhỏ hơn 1024x768 thường là mobile

## 📁 Cấu Trúc Files

```
Assets/
├── Script/
│   └── MobileDetector.cs          # C# script để gọi JavaScript
└── Plugins/
    └── WebGL/
        └── MobileDetection.jslib   # JavaScript library
```

## ✅ Đã Tích Hợp

Code đã được tích hợp vào `HandlePlayer.cs`:
- Tự động sử dụng `MobileDetector.IsMobile` để phát hiện
- Joystick sẽ tự động hiện/ẩn dựa trên kết quả phát hiện
- Touch controls chỉ hoạt động trên mobile

## 🧪 Cách Test

### Trong Unity Editor:
1. Bật **Show Joystick In Editor** trong `HandlePlayer` Inspector
2. Hoặc switch build target sang WebGL → joystick sẽ tự động hiện

### Trên WebGL Build:
1. Build project ra WebGL
2. Mở trên desktop → joystick ẩn, dùng keyboard
3. Mở trên mobile → joystick hiện, dùng touch

## 🔍 Debug

### Xem Log trong Browser Console:
1. Mở game trên trình duyệt
2. Nhấn F12 để mở Developer Tools
3. Vào tab Console
4. Tìm log: `Mobile Detection: {...}`
5. Kiểm tra các giá trị:
   - `isMobileUA`: Có phải mobile theo user agent
   - `hasTouch`: Có hỗ trợ touch
   - `screenSize`: Kích thước màn hình
   - `result`: Kết quả cuối cùng (true/false)

### Xem Log trong Unity:
- Console sẽ hiện: `✅ Joystick đã được kích hoạt` hoặc `❌ Joystick bị ẩn`
- Kiểm tra giá trị `isMobile` trong log

## ⚙️ Tùy Chỉnh

### Thay đổi ngưỡng screen size:
Trong `MobileDetection.jslib`, sửa:
```javascript
var isSmallScreen = window.screen.width < 1024 || window.screen.height < 768;
```

### Thêm từ khóa mobile:
Trong `MobileDetection.jslib`, thêm vào mảng `mobileKeywords`:
```javascript
var mobileKeywords = [
    'android', 'iphone', 'ipad', 'ipod', 
    'blackberry', 'windows phone', 'mobile',
    'webos', 'opera mini', 'iemobile',
    'your-keyword-here'  // Thêm từ khóa mới
];
```

## 🐛 Xử Lý Lỗi

### Lỗi: "IsMobileDevice is not defined"
- **Nguyên nhân**: File `.jslib` không được include trong build
- **Giải pháp**: 
  1. Kiểm tra file `MobileDetection.jslib` có trong `Assets/Plugins/WebGL/`
  2. Đảm bảo file có extension `.jslib` (không phải `.js`)
  3. Rebuild project

### Lỗi: "MobileDetector not found"
- **Nguyên nhân**: Script chưa được compile
- **Giải pháp**: 
  1. Kiểm tra không có lỗi compile trong Console
  2. Đảm bảo file `MobileDetector.cs` có trong project

### Joystick không hiện trên mobile:
- **Kiểm tra**:
  1. Mở Console trong browser (F12)
  2. Xem log "Mobile Detection" → `result` có phải `true` không
  3. Nếu `false`, kiểm tra user agent và screen size
  4. Có thể cần điều chỉnh logic trong `.jslib`

## 📝 Lưu Ý

1. **JavaScript chỉ chạy khi build WebGL**: Trong Editor, sẽ dùng fallback (screen size)
2. **Cache kết quả**: Kết quả được cache, nếu cần detect lại, gọi `MobileDetector.ResetCache()`
3. **HTTPS**: Trên mobile, WebGL yêu cầu HTTPS để hoạt động đúng
4. **Performance**: Detection chỉ chạy 1 lần khi khởi động, không ảnh hưởng FPS

## 🎯 Kết Luận

Giải pháp này cho phép:
- ✅ Phát hiện chính xác mobile device trong WebGL
- ✅ Tự động hiện/ẩn joystick
- ✅ Tự động bật/tắt touch controls
- ✅ Hoạt động trên mọi trình duyệt mobile

Chúc bạn thành công! 🚀

