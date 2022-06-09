# Device.FingerPrint.API

Hướng dẫn start WebApp

 
Khởi động WebAPI
1.Sử dụng cmd trong window
2.Gõ lệnh cd vào folder chứa source WebAPI
3.Gõ lệnh dotnet watch run

4.Như vậy là đã khởi động được WebAPI chứa api trả về token

Khởi động WebApp
1.Sử dụng cmd trong window
2.Gõ lệnh cd vào folder chứa source WebMVC
3.Gõ lệnh dotnet watch run

4.Như vậy là đã khởi động được WebMVC chứa giao diện trang Web



Kết quả:
- Khi truy cập vào API: https://localhost:5001/api/token/3
Sẽ thấy kết quả trả về token và script


- Khi truy cập vào trang web: https://localhost:8001/Home/Index/3 
Sẽ thấy trang web lấy được kết quả của API 


JWT tutorial:
- Đối với những API có check authorization, khi client gọi API sẽ bị trả về Status code 401
- Cách sử dụng JWT:
	+ Client sẽ nhận được JWT từ server thông qua API getToken
	+ Add header: "Authorization: Bearer <JWT>"
	+ Nếu JWT vẫn còn thời hạn thì sẽ server sẽ trả về dữ liệu
