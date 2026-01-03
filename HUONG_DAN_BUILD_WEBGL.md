# Hướng Dẫn Build WebGL cho Biological Museum

## 📋 Tổng Quan

Dự án đã được tối ưu để build WebGL với các cải thiện:
- ✅ Memory size tăng lên 512MB (từ 16MB)
- ✅ Initial memory: 128MB (từ 32MB)
- ✅ Code đã được điều chỉnh để tương thích WebGL tốt hơn
- ✅ Cursor lock được xử lý phù hợp với trình duyệt
- ✅ **Hỗ trợ Mobile/Touch**: Đã thêm touch controls cho điện thoại
  - Touch để xoay camera (vuốt màn hình)
  - Touch để click vào objects
  - Tự động phát hiện mobile device

## 🚀 Các Bước Build

### Bước 1: Mở Unity Editor
1. Mở Unity Hub
2. Chọn project "Biological museum"
3. Đảm bảo Unity version: **2022.3.55f1**

### Bước 2: Chọn Platform WebGL
1. Vào menu: **File > Build Settings...** (hoặc `Ctrl+Shift+B`)
2. Trong danh sách Platform, chọn **WebGL**
3. Nếu chưa có, click **Switch Platform** và đợi Unity chuyển đổi

### Bước 3: Cấu Hình Build (Tùy chọn)
1. Click **Player Settings...** (hoặc `Ctrl+Shift+P`)
2. Trong **Player Settings**, kiểm tra:
   - **Company Name**: DefaultCompany (hoặc tên của bạn)
   - **Product Name**: Biological museum
   - **Default Canvas Scalers**: Phù hợp với kích thước màn hình
   - **WebGL Memory Size**: 512 MB (đã được cấu hình)
   - **Compression Format**: Brotli (khuyến nghị) hoặc Gzip

### Bước 4: Build Project
1. Trong **Build Settings**, đảm bảo scene chính đã được thêm vào "Scenes In Build"
2. Chọn thư mục output (ví dụ: `Build/WebGL`)
3. Click **Build** hoặc **Build And Run**
4. Đợi quá trình build hoàn tất (có thể mất 10-30 phút tùy máy)

## 📁 Cấu Trúc Thư Mục Build

Sau khi build, bạn sẽ có:
```
Build/WebGL/
├── index.html          # File HTML chính
├── Build/              # Các file .data, .wasm, .js
├── TemplateData/       # Assets cho template
└── StreamingAssets/    # Dữ liệu quiz (nếu có)
```

## 🌐 Deploy Lên Web Server

### Option 1: Deploy Lên GitHub Pages (Miễn phí)
1. Tạo repository trên GitHub
2. Upload toàn bộ thư mục `Build/WebGL` lên repository
3. Vào Settings > Pages
4. Chọn branch chứa files và folder root
5. Truy cập: `https://[username].github.io/[repo-name]`

### Option 2: Deploy Lên Netlify (Miễn phí, dễ nhất)
1. Truy cập [netlify.com](https://netlify.com)
2. Kéo thả thư mục `Build/WebGL` vào Netlify
3. Nhận link ngay lập tức (ví dụ: `https://your-project.netlify.app`)

### Option 3: Deploy Lên Vercel
1. Cài đặt Vercel CLI: `npm i -g vercel`
2. Trong thư mục `Build/WebGL`, chạy: `vercel`
3. Làm theo hướng dẫn

### Option 4: Web Server Riêng
1. Upload toàn bộ thư mục `Build/WebGL` lên server
2. Đảm bảo server hỗ trợ:
   - MIME types cho `.wasm`, `.data`, `.js`
   - Gzip/Brotli compression
   - HTTPS (khuyến nghị)

## ⚙️ Cấu Hình Web Server (Nếu cần)

### Apache (.htaccess)
```apache
# Enable compression
<IfModule mod_deflate.c>
    AddOutputFilterByType DEFLATE application/wasm application/javascript application/octet-stream
</IfModule>

# MIME types
AddType application/wasm .wasm
AddType application/octet-stream .data
```

### Nginx
```nginx
location / {
    gzip on;
    gzip_types application/wasm application/javascript application/octet-stream;
    add_header Content-Type application/wasm;
}
```

## 🐛 Xử Lý Lỗi Thường Gặp

### Lỗi: "Memory limit exceeded"
- **Giải pháp**: Tăng `webGLMemorySize` trong ProjectSettings (hiện tại: 512MB)
- Hoặc giảm chất lượng textures/models

### Lỗi: "Cursor lock không hoạt động"
- **Giải pháp**: Đã được xử lý trong code, cursor sẽ lock khi drag chuột
- Trên một số trình duyệt, cần user interaction trước khi lock

### Lỗi: "Build quá lớn"
- **Giải pháp**: 
  - Bật compression (Brotli/Gzip)
  - Tối ưu textures (giảm resolution)
  - Sử dụng Asset Bundles cho nội dung lớn

### Lỗi: "Không load được trên mobile"
- **Giải pháp**: 
  - Code đã hỗ trợ touch input, nhưng performance có thể chậm hơn
  - Test trên thiết bị thật trước khi deploy
  - Giảm chất lượng graphics nếu cần
  - Đảm bảo sử dụng HTTPS (bắt buộc cho WebGL trên mobile)

## 📊 Tối Ưu Performance

### Trước Khi Build:
1. **Tối ưu Models**:
   - Giảm polygon count
   - Sử dụng LOD (Level of Detail)
   - Nén textures (DXT/ETC2)

2. **Tối ưu Textures**:
   - Giảm resolution (1024x1024 thay vì 2048x2048)
   - Sử dụng texture compression
   - Loại bỏ textures không dùng

3. **Tối ưu Code**:
   - Tránh `Update()` không cần thiết
   - Sử dụng object pooling
   - Giảm draw calls

### Sau Khi Build:
1. Kiểm tra kích thước file
2. Test trên nhiều trình duyệt (Chrome, Firefox, Edge)
3. Test trên nhiều thiết bị (desktop, laptop)

## 🔍 Kiểm Tra Build

### Test Local:
1. Cài đặt web server local (ví dụ: Python)
   ```bash
   cd Build/WebGL
   python -m http.server 8000
   ```
2. Mở trình duyệt: `http://localhost:8000`

### Test Checklist:
**Desktop:**
- [ ] Game load được
- [ ] Movement hoạt động (WASD/Arrow keys)
- [ ] Mouse look hoạt động (drag chuột)
- [ ] UI hiển thị đúng
- [ ] Quiz system hoạt động
- [ ] Clickable objects hoạt động
- [ ] Không có lỗi trong Console (F12)

**Mobile (Điện thoại):**
- [ ] Game load được trên trình duyệt mobile
- [ ] Touch để xoay camera hoạt động (vuốt màn hình)
- [ ] Touch để click objects hoạt động
- [ ] UI hiển thị đúng và có thể tương tác
- [ ] Quiz system hoạt động với touch
- [ ] Performance chấp nhận được (có thể chậm hơn desktop)

## 📱 Hỗ Trợ Mobile/Điện Thoại

### ✅ Đã Hỗ Trợ:
- **Touch Input**: Vuốt màn hình để xoay camera
- **Touch Click**: Chạm vào objects để tương tác
- **Tự động phát hiện**: Code tự động nhận biết mobile device qua JavaScript
  - Kiểm tra User Agent (Android, iPhone, iPad, etc.)
  - Kiểm tra Touch Support
  - Kiểm tra Screen Size
- **UI tương thích**: UI hoạt động tốt với touch
- **Joystick tự động**: Joystick tự động hiện trên mobile, ẩn trên desktop

### ⚠️ Lưu Ý Quan Trọng:
1. **Performance**: 
   - WebGL trên mobile **chậm hơn desktop** đáng kể
   - Phù hợp với dự án giáo dục (không cần FPS cao)
   - Nên test trên thiết bị thật trước khi deploy

2. **Trình duyệt Mobile**:
   - **Chrome Android**: Tốt nhất, hỗ trợ đầy đủ
   - **Safari iOS**: Có thể có vấn đề, test kỹ
   - **Firefox Mobile**: Tốt
   - **Samsung Internet**: Tốt

3. **HTTPS Bắt Buộc**:
   - WebGL trên mobile **yêu cầu HTTPS**
   - Không thể chạy trên HTTP localhost
   - Phải deploy lên server có SSL

4. **Kích thước Build**:
   - Mobile có băng thông hạn chế
   - Cố gắng giữ build < 30MB cho mobile
   - Sử dụng compression (Brotli/Gzip)

5. **Điều chỉnh Touch Sensitivity**:
   - Trong `HandlePlayer.cs`, có thể điều chỉnh `touchSensitivity`
   - Mặc định: 2.0 (có thể tăng/giảm tùy nhu cầu)

### 🎮 Cách Sử Dụng Trên Mobile:
1. **Di chuyển**: Sử dụng **Virtual Joystick** ở góc dưới bên trái màn hình
2. **Xoay Camera**: Vuốt một ngón tay trên màn hình (không phải UI và không phải joystick)
3. **Click Object**: Chạm vào object để tương tác

### 📱 Setup Joystick trong Unity:
1. **Thêm Joystick vào Scene**:
   - Mở scene chính
   - Tìm Canvas (hoặc tạo mới: GameObject > UI > Canvas)
   - Kéo một trong các prefab từ `Assets/Joystick Pack/Prefabs/` vào Canvas:
     - **Fixed Joystick**: Joystick cố định ở một vị trí
     - **Floating Joystick**: Joystick xuất hiện tại vị trí touch
     - **Dynamic Joystick**: Joystick di chuyển theo touch
     - **Variable Joystick**: Có thể chuyển đổi giữa 3 loại trên

2. **Đặt vị trí Joystick**:
   - Chọn joystick trong Hierarchy
   - Đặt Anchor ở góc dưới bên trái (Bottom-Left)
   - Điều chỉnh vị trí và kích thước phù hợp

3. **Kết nối Joystick với Code**:
   - Chọn GameObject có script `UIController` (hoặc tạo mới)
   - Kéo joystick vào field **Joystick** trong Inspector
   - Chọn GameObject có script `HandlePlayer`
   - Kéo joystick vào field **Joystick** trong Inspector
   - (Hoặc code sẽ tự động tìm joystick từ UIController)

4. **Test**:
   - Joystick sẽ tự động ẩn trên desktop
   - Chỉ hiện trên mobile/WebGL mobile
   - Test bằng cách chạy game và vuốt joystick

## 📝 Lưu Ý Quan Trọng

1. **HTTPS**: **Bắt buộc** cho WebGL trên mobile, một số tính năng desktop cũng yêu cầu
2. **Browser Support**: 
   - **Desktop**: Chrome/Edge (tốt nhất), Firefox (tốt), Safari (có thể có vấn đề)
   - **Mobile**: Chrome Android (tốt nhất), Safari iOS (test kỹ), Firefox Mobile (tốt)
3. **Mobile Performance**: WebGL trên mobile có hạn chế, nhưng đã được tối ưu cho dự án giáo dục
4. **File Size**: 
   - Desktop: Cố gắng < 50MB
   - Mobile: Cố gắng < 30MB để load nhanh hơn

## 🎯 Kết Luận

Dự án đã sẵn sàng để build WebGL! Chỉ cần:
1. Build trong Unity
2. Upload lên web server
3. Chia sẻ link với người dùng

Chúc bạn build thành công! 🚀

