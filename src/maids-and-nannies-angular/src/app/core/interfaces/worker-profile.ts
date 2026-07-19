import { Entity } from "./entity";
import { WorkerDocument } from "./worker-document";
import { WorkerSpecializationSpec } from "./worker-specialization-spec";

export interface WorkerProfile extends Entity{
    userId:string,
    fullName:string,
    email:string,
    phoneNumber:string | null,
    whatsAppNumber:string | null,
    nationalityId:number,
    nationalIdNumber:string,
    passportNumber:string | null,
    passportExpiryDate:Date | null,
    passportCountry:string | null,
    bio:string | null,
    experienceYears:number,
    previousEmployer:string,
    languages:string | null,
    workerSpecializationSpecs:WorkerSpecializationSpec[],
    isLiveIn:boolean,
    hourlyRate:number,
    monthlyRate:number,
    currency:number,
    stateId:number | null,
    countryId:number | null,
    cityId:number | null,
    address:string,
    averageRating:number,
    totalReviews:number,
    isAvailable:boolean,
    verificationStatus:number,
    verifiedAt:Date | null,
    verifiedBy:string | null,
    documents:WorkerDocument[]    
}