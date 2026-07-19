import { Injectable, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private readonly STORAGE_KEY = 'app_language';
  readonly supportedLanguages = ['ar', 'en'];
  private translate = inject(TranslateService);

  constructor() {
    this.initLanguage();
  }

  private initLanguage(): void {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    let lang = 'ar';

    if (stored && this.supportedLanguages.includes(stored)) {
      lang = stored;
    }

    this.translate.setFallbackLang('ar');
    this.translate.use(lang);
    this.updateDirection(lang);
  }

  setLanguage(lang: string): void {
    if (this.supportedLanguages.includes(lang)) {
      localStorage.setItem(this.STORAGE_KEY, lang);
      this.translate.use(lang);
      this.updateDirection(lang);
      window.location.reload();
    }
  }

  private updateDirection(lang: string): void {
    const dir = lang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);
  }

  getCurrentLanguage(): string {
    return this.translate.getCurrentLang() || 'ar';
  }

  isRTL(): boolean {
    return this.getCurrentLanguage() === 'ar';
  }
}
