# Giao thức kết nối PLC Inovance

Dự án này cung cấp giao thức để kết nối với các dòng PLC Inovance bao gồm **AM600**, **H3U**, **H5U** và **EASY**. Bạn có thể sử dụng trực tiếp trên nền tảng phát triển của mình.

## Tính năng

Dự án hỗ trợ các tính năng sau:
- Kết nối với PLC Inovance qua Ethernet (sử dụng Modbus TCP).
- Đọc và ghi dữ liệu vào thanh ghi PLC.
- Đặt và xóa bit PLC.
- Thực thi các lệnh PLC.
- Giám sát trạng thái PLC.
- Xử lý lỗi.

## Yêu cầu

Để sử dụng dự án, bạn cần:
1. Một PLC Inovance từ dòng **AM600**, **H3U**, **H5U**, hoặc **EASY**.
2. Một máy tính có kết nối Ethernet.
3. **Visual Studio** (hoặc IDE tương thích với ngôn ngữ lập trình dự án).

## Cài đặt

### Hướng dẫn chung:
1. Tải xuống mã nguồn của dự án từ [kho lưu trữ GitHub](#) (thay link GitHub của bạn).
2. Giải nén mã nguồn vào một thư mục trên máy tính.
3. Tích hợp mã nguồn vào dự án của bạn tùy thuộc vào ngôn ngữ lập trình và môi trường phát triển.

### Cấu hình mạng:
- Đảm bảo rằng máy tính của bạn và PLC cùng nằm trong một mạng.
- Thiết lập địa chỉ IP, subnet mask phù hợp.

## Cách sử dụng

1. **Kết nối với PLC**:
   - Khởi tạo kết nối với địa chỉ IP và cổng (thường là **502**) của PLC.

2. **Sử dụng API**:
   - Đọc/ghi dữ liệu từ thanh ghi.
   - Đặt và xóa bit.
   - Theo dõi trạng thái PLC.

