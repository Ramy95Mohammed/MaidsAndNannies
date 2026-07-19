import { Entity } from "./entity";

export interface WorkerDocument extends Entity {
    workerId:number,
    type:number,
    documentImageUrl:string,
    verificationStatus:number,    
}
