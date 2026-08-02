//src/modules/PhoneBook/api/PhoneBookApi.ts
import getAPI from "@/core/api/axiosClient";


import { PhoneBookEmploymentDto } from "../models/PhoneBookEmploymentDto";
const API_MODULE = "phonebook";

export const phonebookApi = {

 // دریافت (GET)
  GetList: async (organUnitId?: string): Promise<PhoneBookEmploymentDto[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PhoneBookEmploymentDto[]>(
      "/api/People/PhoneBook/GetList",
      { 
        params: { organUnitId },
        withCredentials: true 
      }
    );
    console.log(response)
    return response.data;
  }

};


/*
من توی بک اند این کنترلر رو ساختم:
 [ApiController]
 [Route("api/People/[controller]")]
 public class PhoneBookController : BaseController
 {

     [HttpGet("GetList")]
     public async Task<IActionResult> GetSelectionList([FromQuery] GetPhoneBookListQuery? request = null)
     {
         var result = await Mediator.Send(request);
         return HandleResult(result);
     }



 }
که دیتاش از این میاد:
public record GetPhoneBookListQuery(Guid? organUnitId = null)
 : IRequest<Result<IReadOnlyList<PhoneBookEmploymentDto>>>;public class GetPhoneBookListQueryHandler
    : IRequestHandler<GetPhoneBookListQuery, Result<IReadOnlyList<PhoneBookEmploymentDto>>>
{
    private readonly IPhoneBookInternalService _phoneBookInternalService;
    private readonly ILogger<GetPhoneBookListQueryHandler> _logger;

    public GetPhoneBookListQueryHandler(
        IPhoneBookInternalService phoneBookInternalService,
    ILogger<GetPhoneBookListQueryHandler> logger)
    {
        _phoneBookInternalService = phoneBookInternalService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PhoneBookEmploymentDto>>> Handle(
        GetPhoneBookListQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting PhoneBook Info List:");

            var list = await _phoneBookInternalService.GetPhoneBookListAsync(request.organUnitId);
            return Result<IReadOnlyList<PhoneBookEmploymentDto>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PhoneBook Info List");
            return Result<IReadOnlyList<PhoneBookEmploymentDto>>.Fail(ex.Message);
        }
    }
}
اینم مدلمون تو فرانت:
// models/PhoneBookEmploymentDto.ts

export interface PhoneBookEmploymentDto {
  EmploymentCode: string;
  postCode: string;
  FirstName?: string | null;
  LastName?: string | null;
  FullName?: string | null;
  OrganizationUnitsName?: string | null;
  JobTitleName?: string | null;
  JobLevelTitle?: string | null;
  LocationTitle?: string | null;
  
  ContactSummary?: string | null;
  HasMultipleContacts?: boolean | false;

  Contacts?: ContactDetailDto[] | null;
}
export interface ContactDetailDto {
  Title: string;
  Value: string;
  Type?: PhoneBookContactTypeEnum | null;
  Source?: PhoneBookContactSourceEnum | null;
 

}

export enum PhoneBookContactTypeEnum {
  Mobile,
    Phone,
    Email,
    Fax,
    Address
 

}

export enum PhoneBookContactSourceEnum {
 Personal,   
      Organizational 
 

}

حالا توی فرانت اند میخایم نمایشش بدیم.
فرانتمو ری اکت هست و فایل زیر رو برای خوندن api دارم
// modules/phonebook/api/phonebookApi.ts
import getAPI from "@/core/api/axiosClient";


import { PhoneBookEmploymentDto } from "../models/PhoneBookEmploymentDto";
const API_MODULE = "phonebook";

export const phonebookApi = {

 // دریافت (GET)
  GetList: async (): Promise<PhoneBookEmploymentDto[]> => {
    
    const api = getAPI(API_MODULE);
    
    const response = await api.get<PhoneBookEmploymentDto[]>(
      "/api/phonebook/OrgChart/GetList",
      {  withCredentials: true }
    );
    console.log(response)
    return response.data;
  }

};
حالا میخایم توی صفحه زیر با همون ساختار نمایشی که گفتم نشون بدیم اطلاعات و دفترچه تلفنمون رو:
// src/modules/PhoneBook/pages/Post/PhoneBookPage.tsx

import {
  phonebookApi} from "../../api/PhoneBookApi";
.
.
.
ادامه کد رو بنویس

*/