import { Routes } from '@angular/router';

export default <Routes>[
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    {
  path: 'dashboard',
  loadComponent: () =>import('./dashboard/homeowner-dashboard').then(m => m.HomeownerDashboard)},
    { path: 'profile', loadComponent: () => import('./profile/homeowner-profile').then(m => m.HomeownerProfileComponent) },
    { path: 'workers', loadComponent: () => import('./workers/worker-search').then(m => m.WorkerSearch) },
    { path: 'workers/:id', loadComponent: () => import('./workers/worker-detail').then(m => m.WorkerDetail)  },
    { path: 'bookings', loadComponent: () => import('./bookings/homeowner-bookings').then(m => m.HomeownerBookings)  },
    { path: 'bookings/:id', loadComponent: () => import('./bookings/booking-detail').then(m => m.BookingDetail)  }
];
