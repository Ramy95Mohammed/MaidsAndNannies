export interface Country {
    id: number;
    name: string;
    iso3: string;
    numericCode: string;
    iso2: string;
    phoneCode: string;
    capital: string | null;
    currency: string | null;
    currencyName: string | null;
    currencySymbol: string | null;
    tld: string | null;
    native: string | null;
    region: string | null;
    regionId: number | null;
    subregion: string | null;
    subregionId: number | null;
    nationality: string | null;
    timezones: string;
    translations: string;
    latitude: string;
    longitude: string;
    emoji: string;
    emojiU: string;
    createdAt: string;
    updatedAt: string;
    flag: number | null;
    wikiDataId: string | null;
}