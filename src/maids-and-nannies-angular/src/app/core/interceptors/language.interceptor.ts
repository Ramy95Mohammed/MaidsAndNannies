import { HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { LanguageService } from "../services/language.service";
import { TranslateService } from "@ngx-translate/core";

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
   const currentLanguage = localStorage.getItem('app_language') ?? 'ar';
  req = req.clone({
    setHeaders: {
      currentLanguage: currentLanguage
    }
  });

  return next(req);
};