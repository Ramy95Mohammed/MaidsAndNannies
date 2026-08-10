import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, isDevMode, provideAppInitializer } from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import Aura from '@primeuix/themes/aura';
import { providePrimeNG } from 'primeng/config';
import { appRoutes } from './app.routes';
import { authInterceptor } from './app/core/interceptors/auth.interceptor';
import { MessageService } from 'primeng/api';
import { languageInterceptor } from '@/core/interceptors/language.interceptor';

export const appConfig: ApplicationConfig = {
    providers: [
        // provideAppInitializer(() => {
        //     if ('serviceWorker' in navigator) {
        //         navigator.serviceWorker
        //             .register('/sw.js')
        //             .catch(error => {
        //                 console.error('Service Worker registration failed:', error);
        //             });
        //     }
        // }),
        MessageService,
        provideRouter(appRoutes, withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }), withEnabledBlockingInitialNavigation()),
        provideHttpClient(withFetch(), withInterceptors([authInterceptor , languageInterceptor])),
        provideAnimationsAsync(),
        providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } } }),
        provideTranslateService({
            fallbackLang: 'ar',
            lang: 'ar'
        }),
        provideTranslateHttpLoader({
            prefix: './i18n/',
            suffix: '.json'
        })
    ]
};


