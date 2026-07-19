import { Routes } from '@angular/router';

export default <Routes>[
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', loadComponent: () => import('./dashboard/admin-dashboard') },
    { path: 'homeowners', loadComponent: () => import('./homeowners/admin-homeowners') },
    { path: 'workers', loadComponent: () => import('./workers/admin-workers') },
    { path: 'payments', loadComponent: () => import('./payments/admin-payments') }
];
