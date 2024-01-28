using CRUD_API_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharpCore.Pdf.IO;

namespace CRUD_API_Assignment.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    
    public class UserController:ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IMapper mapper, IConfiguration configuration)
        {
            _mapper = mapper;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpGet("GetAllUsers"), Authorize(Roles="Admin")]
        public async Task< ActionResult<ServiceResponse<List<GetUserResponseDto>>>> GetUsers()
        {
            return Ok( await _userService.GetAllUsers());
        }

        [HttpGet("GetUserById")]
        public async Task<ActionResult<ServiceResponse< GetUserResponseDto>>> GetUserById(string id=null)
        {
            if(string.IsNullOrEmpty(id))
            return BadRequest("Invalid userid");
            var response=await _userService.GetUserById(id);
            if(response.Data is null)
            return NotFound(response);
            return Ok(response);
        }

        
        [HttpPost("AddUser"), Authorize]
        public async Task<ActionResult<ServiceResponse<string>>> AddUser(AddUserResquestDto user)
        {
            if(string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Password)  || user.Age==0 || user.Hobbies.Count<0  )
            return BadRequest("Invalid user details");

            var response=await _userService.AddUser(user,user.Password);
            return Ok(response);

        }

        [HttpPost("AddRole"), Authorize(Roles="Admin")]
        public async Task<ActionResult<ServiceResponse<Role>>> AddRole(Role role)
        {
            if(string.IsNullOrEmpty(role.Name) )
            return BadRequest("Invalid user details");

            var response=await _userService.AddRole(role);
            return Ok(response);

        }

          [HttpPost("AssignRoleToUser"), Authorize(Roles="Admin")]
        public async Task<ActionResult<ServiceResponse<Role>>> AssignRoleToUser(AddUserRole obj)
        {
            if(string.IsNullOrEmpty(obj.UserId) || obj.RoleIds.Count<0 )
            return BadRequest("Invalid user details");

            var response=await _userService.AssignRoleToUser(obj);
            return Ok(response);

        }


        [AllowAnonymous] 
        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(UserLoginRequestDto request)
        {
            if(string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password)  )
            return BadRequest("Invalid user details");

            var response=await _userService.Login(request.UserName,request.Password);
            if(response.Success==false)
            return NotFound(response);
            return Ok(response);

        }

       
       [HttpPut, Authorize]
        public async Task<ActionResult<ServiceResponse<GetUserResponseDto>>> UpdateUser(UpdateUserRequestDto updatedUser)
        {
            var response=await _userService.UpdateUser(updatedUser);
            if(response.Data is null)
            {
                return NotFound(response);
            }
            return Ok(response);
        }

        
        [HttpDelete(), Authorize(Roles="Admin")]

        public async Task<ActionResult<ServiceResponse<List<GetUserResponseDto>>>> DeleteUser(string id=null)
        {
             if(string.IsNullOrEmpty(id))
            return BadRequest("Invalid userid");
            var response=await _userService.DeleteUser(id);
            if(response.Success==false)
            return NotFound(response);
        
            return NoContent();
        }

        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                var users = await _userService.GetAllUsers();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("users");
                    // add header row

                    worksheet.Cells["A1"].Value = "UserId";
                    worksheet.Cells["B1"].Value = "UserName";
                    worksheet.Cells["D1"].Value = "Age";
                    worksheet.Cells["E1"].Value = "isAdmin";
                    worksheet.Cells["F1"].Value = "Hobbies";

                    //add data row
                    int row = 2;

                    if (users.Data != null)
                    {

                        foreach (var user in users.Data)
                        {

                            worksheet.Cells[$"A{row}"].Value = user.Id;
                            worksheet.Cells[$"B{row}"].Value = user.UserName;
                            worksheet.Cells[$"D{row}"].Value = user.Age;
                            worksheet.Cells[$"E{row}"].Value = user.isAdmin;
                            worksheet.Cells[$"F{row}"].Value = user.Hobbies;
                            row++;
                        }

                    }

                    using (var stream = new MemoryStream())
                    {
                        
                        package.SaveAs(stream);

                        // Set the position of the stream back to the beginning
                        stream.Position = 0;

                        var password = _configuration.GetSection("Password").GetSection("mypassword").Value;

                        var encrypExcelStream = EncryptExcel(stream, password);

                        // Return the Excel file as a FileStreamResult
                        return File(encrypExcelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");

                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private MemoryStream EncryptExcel(MemoryStream inputExcelStream, string? password)
        {
            // Load the existing Excel package from the input stream
            using (var package = new ExcelPackage(inputExcelStream))
            {
                // Set encryption settings for the Excel package
                package.Encryption.Password = password;
                // Save the encrypted Excel package to a MemoryStream
                var encryptedExcelStream = new MemoryStream();
                package.SaveAs(encryptedExcelStream);

                // Reset the position of the MemoryStream
                encryptedExcelStream.Position = 0;

                return encryptedExcelStream;
            }

        }

        [HttpGet("ExportToPDF")]
        public async Task<IActionResult> ExportToPDF()
        {
            var users = await _userService.GetAllUsers();

            var document = new PdfSharpCore.Pdf.PdfDocument();
            
            string htmlContent = "<h1>User Details</h1>";
            htmlContent += "<table>";
            htmlContent += "<thead>";
            htmlContent += "<tr>";
            htmlContent += "<td style='border:1px solid  #000'>UserId</td>";
            htmlContent += "<td style='border:1px solid  #000'>UserName</td>";
            htmlContent += "<td style='border:1px solid  #000'>Age</td>";
            htmlContent += "<td style='border:1px solid  #000'>IsAdmin</td>";
            htmlContent += "</tr>";
            htmlContent += "</thead>";
            htmlContent += "<tbody>";
            htmlContent += "<tr>";
            if (users.Data != null)
            {
                foreach (var item in users.Data)
                {
                    htmlContent += "<td>"+item.Id+"</td>";
                    htmlContent += "<td>"+item.UserName+"</td>";
                    htmlContent += "<td>"+item.Age+"</td>";
                    htmlContent += "<td>"+item.isAdmin+"</td>";
                }
            }

            htmlContent += "</tr>";
            
            htmlContent += "</tbody>";
            htmlContent += "</table>";

            PdfGenerator.AddPdfPages(document, htmlContent, PdfSharpCore.PageSize.A4);

            string password = _configuration.GetSection("Password").GetSection("mypassword").Value!;
            using (var stream = new MemoryStream())
            {
                document.Save(stream);
                stream.Position = 0;
                var encryptedStream = PdfEncryption(stream, password);
                return File(encryptedStream, "application/pdf", "users.pdf");

            }

        }

        private MemoryStream PdfEncryption(MemoryStream inputPdfStream, string password)
        {
            var document =PdfSharpCore.Pdf.IO.PdfReader.Open(inputPdfStream, PdfDocumentOpenMode.Modify);
            var securitySettings = document.SecuritySettings;
            securitySettings.OwnerPassword = password;
            securitySettings.UserPassword = password;
            securitySettings.PermitAccessibilityExtractContent = false;
            securitySettings.PermitAnnotations = false;
            securitySettings.PermitAssembleDocument = false;
            securitySettings.PermitExtractContent = false;
            securitySettings.PermitFormsFill = true;
            securitySettings.PermitFullQualityPrint = false;
            securitySettings.PermitModifyDocument = false;
            securitySettings.PermitPrint = false;

            // Save the document to a MemoryStream
            var encryptedPdfStream = new MemoryStream();
            document.Save(encryptedPdfStream, false);
            encryptedPdfStream.Position = 0;

            return encryptedPdfStream;

        }



    }

}