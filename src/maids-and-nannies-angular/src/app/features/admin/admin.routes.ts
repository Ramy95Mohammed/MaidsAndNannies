import { Routes } from '@angular/router';

export default <Routes>[
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', loadComponent: () => import('./dashboard/admin-dashboard').then(m=>m.AdminDashboard) },
    { path: 'homeowners', loadComponent: () => import('./homeowners/admin-homeowners').then(m=>m.AdminHomeowners) },
    { path: 'workers', loadComponent: () => import('./workers/admin-workers').then(m=>m.AdminWorkers) },
    { path: 'payments', loadComponent: () => import('./payments/admin-payments').then(m=>m.AdminPayments) },
    { path: 'bookings', loadComponent: () => import('./bookings/admin-bookings').then(m => m.AdminBookings) },
    { path: 'subscriptions', loadComponent: () => import('./subscriptions/admin-subscriptions').then(m => m.AdminSubscriptions) },
    { path: 'currencies', loadComponent: () => import('./currencies/admin-currencies').then(m => m.AdminCurrencies) },
        { path: 'register-homeowner', loadComponent: () => import('./register-homeowner/admin-register-homeowner').then(m => m.AdminRegisterHomeowner) },
    { path: 'settings', loadComponent: () => import('./settings/admin-settings').then(m => m.AdminSettings) },
    { path: 'job-posts', loadComponent: () => import('./jobs/admin-job-posts').then(m => m.AdminJobPosts) },
];
