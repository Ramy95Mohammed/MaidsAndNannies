export interface City {
     id: number;
    name: string;
    state_id: number | null;
    state_code: string | null;
    country_id: number | null;
    country_code: string | null;
    latitude: string | null;
    longitude: string | null;
    created_at: string | null;
    updated_at: string | null;
    flag: number | null;
    wikiDataId: string | null;
}
