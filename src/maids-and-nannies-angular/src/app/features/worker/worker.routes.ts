import { Routes } from '@angular/router';

export default <Routes>[
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'dashboard', loadComponent: () => import('./dashboard/worker-dashboard').then(m => m.WorkerDashboard) },
    { path: 'profile', loadComponent: () => import('./profile/worker-profile').then(m => m.WorkerProfileComponent) },
    { path: 'bookings', loadComponent: () => import('./bookings/worker-bookings').then(m => m.WorkerBookings) }
];
