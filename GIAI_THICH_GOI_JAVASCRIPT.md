# Giải Thích: JavaScript Được Gọi Ở Đâu?

## 🔄 Luồng Gọi JavaScript

### 1. **Điểm Bắt Đầu: HandlePlayer.cs**

```csharp
// Dòng 60 trong HandlePlayer.cs
isMobile = MobileDetector.IsMobile;
```

Khi game khởi động, `HandlePlayer` gọi `MobileDetector.IsMobile` để kiểm tra xem có phải mobile không.

---

### 2. **MobileDetector.cs - Property IsMobile**

```csharp
// Dòng 15-33 trong MobileDetector.cs
public static bool IsMobile
{
    get
    {
        if (cachedIsMobile.HasValue)
            return cachedIsMobile.Value;  // Trả về kết quả đã cache
        
        // Nếu chưa có, gọi DetectMobile()
        cachedIsMobile = DetectMobile();  // ← GỌI Ở ĐÂY
        return cachedIsMobile.Value;
    }
}
```

Property `IsMobile` sẽ gọi hàm `DetectMobile()` nếu chưa có kết quả cache.

---

### 3. **MobileDetector.cs - Hàm DetectMobile()**

```csharp
// Dòng 49-72 trong MobileDetector.cs
static bool DetectMobile()
{
    // Kiểm tra platform native trước
    if (Application.isMobilePlatform)
        return true;
    
    // WebGL - dùng JavaScript để phát hiện
    #if UNITY_WEBGL && !UNITY_EDITOR
    return IsMobileWebGL();  // ← GỌI Ở ĐÂY (chỉ khi build WebGL)
    #else
    // Trong Editor, dùng screen size
    return Screen.width < 1024 || Screen.height < 768;
    #endif
}
```

Nếu đang chạy trên WebGL (không phải Editor), sẽ gọi `IsMobileWebGL()`.

---

### 4. **MobileDetector.cs - Hàm IsMobileWebGL()**

```csharp
// Dòng 74-90 trong MobileDetector.cs
#if UNITY_WEBGL && !UNITY_EDITOR
// Khai báo function JavaScript
[System.Runtime.InteropServices.DllImport("__Internal")]
private static extern bool IsMobileDevice();  // ← KHAI BÁO Ở ĐÂY

static bool IsMobileWebGL()
{
    try
    {
        return IsMobileDevice();  // ← GỌI JAVASCRIPT Ở ĐÂY!
    }
    catch
    {
        // Fallback nếu JavaScript không hoạt động
        return Screen.width < 1024 || Screen.height < 768;
    }
}
#endif
```

**Đây là nơi JavaScript được gọi!**

- `[DllImport("__Internal")]` là attribute đặc biệt của Unity để gọi JavaScript
- `IsMobileDevice()` là tên function JavaScript trong file `.jslib`
- Khi gọi `IsMobileDevice()`, Unity sẽ tự động tìm và gọi function JavaScript tương ứng

---

### 5. **MobileDetection.jslib - JavaScript Function**

```javascript
// File: Assets/Plugins/WebGL/MobileDetection.jslib
mergeInto(LibraryManager.library, {
    IsMobileDevice: function () {  // ← FUNCTION JAVASCRIPT Ở ĐÂY
        // Kiểm tra user agent, touch support, screen size
        var isMobile = ...;
        return isMobile ? 1 : 0;  // Trả về 1 (true) hoặc 0 (false)
    }
});
```

Unity tự động include file `.jslib` khi build WebGL và map function `IsMobileDevice` từ C# sang JavaScript.

---

## 📊 Sơ Đồ Luồng

```
HandlePlayer.cs (dòng 60)
    ↓
    isMobile = MobileDetector.IsMobile
    ↓
MobileDetector.cs - Property IsMobile (dòng 31)
    ↓
    cachedIsMobile = DetectMobile()
    ↓
MobileDetector.cs - DetectMobile() (dòng 59)
    ↓
    return IsMobileWebGL()  (chỉ khi UNITY_WEBGL && !UNITY_EDITOR)
    ↓
MobileDetector.cs - IsMobileWebGL() (dòng 83)
    ↓
    return IsMobileDevice()  ← GỌI JAVASCRIPT Ở ĐÂY!
    ↓
MobileDetection.jslib - IsMobileDevice() (dòng 5)
    ↓
    Kiểm tra user agent, touch, screen size
    ↓
    return 1 hoặc 0 (true/false)
    ↓
Kết quả trả về C# → Cache → Sử dụng
```

---

## 🔍 Chi Tiết Kỹ Thuật

### `[DllImport("__Internal")]` là gì?

- `DllImport` là attribute của C# để gọi external functions
- `"__Internal"` là tên đặc biệt của Unity để gọi JavaScript trong WebGL
- Unity sẽ tự động tìm function JavaScript có cùng tên trong các file `.jslib`

### File `.jslib` được xử lý như thế nào?

1. **Khi Build WebGL**: Unity tự động tìm tất cả file `.jslib` trong `Assets/Plugins/WebGL/`
2. **Compile**: Unity compile JavaScript code và merge vào WebGL build
3. **Map Functions**: Unity tạo mapping giữa C# function và JavaScript function
4. **Runtime**: Khi C# gọi `IsMobileDevice()`, Unity tự động gọi JavaScript function tương ứng

---

## ✅ Tóm Tắt

**JavaScript được gọi ở:**
- **File**: `Assets/Script/MobileDetector.cs`
- **Dòng**: 83 trong hàm `IsMobileWebGL()`
- **Code**: `return IsMobileDevice();`

**JavaScript function nằm ở:**
- **File**: `Assets/Plugins/WebGL/MobileDetection.jslib`
- **Function**: `IsMobileDevice: function () { ... }`

**Khi nào được gọi:**
- Khi `HandlePlayer` khởi động và gọi `MobileDetector.IsMobile`
- Chỉ chạy khi build WebGL (không chạy trong Editor)
- Chỉ chạy 1 lần và cache kết quả

---

## 🧪 Cách Kiểm Tra

1. **Build WebGL** và mở trong browser
2. **Mở Console** (F12)
3. **Tìm log**: `Mobile Detection: {...}`
4. Log này được in từ JavaScript function `IsMobileDevice()`

Nếu thấy log này → JavaScript đã được gọi thành công! ✅

