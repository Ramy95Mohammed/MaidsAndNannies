


// export  function  waLink(phone: string | null, text: string): string {
//     const digits = (phone ?? '').replace(/[^0-9]/g, '');
//     const intl = digits.startsWith('0') ? '2' + digits : digits;
//     return `https://wa.me/${intl}?text=${encodeURIComponent(text)}`;
// }

export function waLink(phone: string | null, text: string): string {
    const digits = (phone ?? '').replace(/[^0-9]/g, '');
    const intl = digits.startsWith('0') ? '2' + digits : digits;
    const url = `whatsapp://send?phone=${intl}&text=${encodeURIComponent(text)}`;
    return url;
}