# Giao thức kết nối PLC Inovance

Dự án này cung cấp giao thức để kết nối với các dòng PLC Inovance bao gồm **AM600**, **H3U**, **H5U** và **EASY**. Bạn có thể sử dụng trực tiếp trên nền tảng phát triển của mình.

## Tính năng

Dự án hỗ trợ các tính năng sau:

*   Kết nối với PLC Inovance qua Ethernet (sử dụng Modbus TCP).
*   Đọc và ghi dữ liệu vào thanh ghi PLC.
*   Đặt và xóa bit PLC.
*   Thực thi các lệnh PLC.
*   Giám sát trạng thái PLC.
*   Xử lý lỗi.

## Yêu cầu

Để sử dụng dự án, bạn cần:

1. Một PLC Inovance từ dòng **AM600**, **H3U**, **H5U**, hoặc **EASY**.
2. bạn có thể dùng AutoShop để mô phòng các dòng plc Inovance H5U, Easy (H3U không hỗ trợ mô phỏng).
3. **Visual Studio** (hoặc IDE tương thích với ngôn ngữ lập trình dự án).

## Cài đặt

### Hướng dẫn chung:

1. Tải xuống mã nguồn của dự án từ [Liên kết GitHub](https://github.com/mipu1711/Protocol-Inovance).
2. Tích hợp mã nguồn vào dự án của bạn tùy thuộc vào ngôn ngữ lập trình và môi trường phát triển.


## Cách sử dụng

1. **Kết nối với PLC**:

    *   Khởi tạo kết nối với địa chỉ IP và cổng (thường là **502**) của PLC.
2. **Sử dụng API**:

    *   Đọc/ghi dữ liệu từ thanh ghi.
    *   Đặt và xóa bit.
    *   Theo dõi trạng thái PLC.


    Đóng kết nối khi không sử dụng.

## Tác giả

Dự án được phát triển bởi **Hùng Nguyễn**.

## Liên hệ

Nếu có bất kỳ câu hỏi hoặc nhận xét nào, vui lòng liên hệ:

*   Email: viethungxzc@gmail.com
*   GitHub: [Liên kết GitHub](https://github.com/mipu1711/Protocol-Inovance)
