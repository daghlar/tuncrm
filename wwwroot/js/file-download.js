// File download functions for TunCRM
window.downloadFile = function(fileName, base64Data) {
    try {
        const link = document.createElement('a');
        link.href = 'data:application/octet-stream;base64,' + base64Data;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        console.log('File downloaded:', fileName);
    } catch (error) {
        console.error('Download error:', error);
        alert('Dosya indirilirken hata oluştu: ' + error.message);
    }
};

window.downloadFileFromStream = function(fileName, contentType, data) {
    try {
        const blob = new Blob([data], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        console.log('File downloaded:', fileName);
    } catch (error) {
        console.error('Download error:', error);
        alert('Dosya indirilirken hata oluştu: ' + error.message);
    }
};

// Alert function
window.alert = function(message) {
    alert(message);
};

// Confirm function
window.confirm = function(message) {
    return confirm(message);
};

console.log('File download functions loaded successfully');