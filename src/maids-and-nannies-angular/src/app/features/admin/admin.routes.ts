import { Routes } from '@angular/router';

export default <Routes>[
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', loadComponent: () => import('./dashboard/admin-dashboard').then(m=>m.AdminDashboard) },
    { path: 'homeowners', loadComponent: () => import('./homeowners/admin-homeowners').then(m=>m.AdminHomeowners) },
    { path: 'workers', loadComponent: () => import('./workers/admin-workers').then(m=>m.AdminWorkers) },
    { path: 'payments', loadComponent: () => import('./payments/admin-payments').then(m=>m.AdminPayments) },
    { path: 'bookings', loadComponent: () => import('./bookings/admin-bookings').then(m => m.AdminBookings) }
];
