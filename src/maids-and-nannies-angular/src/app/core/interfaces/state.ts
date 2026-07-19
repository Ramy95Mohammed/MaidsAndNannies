export interface State {
    id: number;
    name: string;
    country_id: number | null;
    country_code: string | null;
    fips_code: string | null;
    iso2: string | null;
    type: string | null;
    latitude: string | null;
    longitude: string | null;
    created_at: string | null;
    updated_at: string | null;
    flag: number | null;
    wikiDataId: string | null;
}