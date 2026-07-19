import { Entity } from "./entity";

export interface WorkerSpecializationSpec extends Entity {
    workerProfileId:number,
    workerSpecialization:number
}
