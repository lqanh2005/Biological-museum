// JavaScript library để phát hiện mobile device trong WebGL
// File này sẽ được Unity tự động include khi build WebGL

mergeInto(LibraryManager.library, {
    IsMobileDevice: function () {
        // Kiểm tra user agent
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;
        
        // Danh sách các từ khóa mobile
        var mobileKeywords = [
            'android', 'iphone', 'ipad', 'ipod', 
            'blackberry', 'windows phone', 'mobile',
            'webos', 'opera mini', 'iemobile'
        ];
        
        // Kiểm tra user agent
        var isMobileUA = false;
        var uaLower = userAgent.toLowerCase();
        for (var i = 0; i < mobileKeywords.length; i++) {
            if (uaLower.indexOf(mobileKeywords[i]) !== -1) {
                isMobileUA = true;
                break;
            }
        }
        
        // Kiểm tra touch support
        var hasTouch = 'ontouchstart' in window || 
                      navigator.maxTouchPoints > 0 || 
                      navigator.msMaxTouchPoints > 0;
        
        // Kiểm tra screen size (mobile thường < 1024px)
        var isSmallScreen = window.screen.width < 1024 || window.screen.height < 768;
        
        // Kết hợp các điều kiện
        // Nếu có user agent mobile HOẶC (có touch VÀ màn hình nhỏ)
        var isMobile = isMobileUA || (hasTouch && isSmallScreen);
        
        // Log để debug (có thể xóa trong production)
        console.log('Mobile Detection:', {
            userAgent: userAgent,
            isMobileUA: isMobileUA,
            hasTouch: hasTouch,
            screenSize: window.screen.width + 'x' + window.screen.height,
            isSmallScreen: isSmallScreen,
            result: isMobile
        });
        
        return isMobile ? 1 : 0;
    }
});

