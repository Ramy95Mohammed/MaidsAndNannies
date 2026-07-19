import { Routes } from '@angular/router';
import { AppLayout } from './app/layout/component/app.layout';
import { Documentation } from './app/pages/documentation/documentation';
import { Landing } from './app/pages/landing/landing';
import { Notfound } from './app/pages/notfound/notfound';
import { AuthGuard } from './app/core/guards/auth.guard';

export const appRoutes: Routes = [
    {
        path: '',
        canActivate: [AuthGuard],
        component: AppLayout,
        children: [
            { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
            { path: 'uikit', loadChildren: () => import('./app/pages/uikit/uikit.routes') },
            { path: 'documentation', component: Documentation },
            { path: 'pages', loadChildren: () => import('./app/pages/pages.routes') },
            {
                path: 'admin',
                loadChildren: () => import('./app/features/admin/admin.routes'),
                canActivate: [AuthGuard],
                data: { roles: ['Admin'] }
            },
            {
                path: 'homeowner',
                loadChildren: () => import('./app/features/homeowner/homeowner.routes'),
                canActivate: [AuthGuard],
                data: { roles: ['Homeowner'] }
            },
            {
                path: 'worker',
                loadChildren: () => import('./app/features/worker/worker.routes'),
                canActivate: [AuthGuard],
                data: { roles: ['Worker'] }
            }
        ]
    },
    { path: 'landing', component: Landing },
    { path: 'notfound', component: Notfound },
    { path: 'auth', loadChildren: () => import('./app/pages/auth/auth.routes') },
    { path: '**', redirectTo: '/notfound' }
];
