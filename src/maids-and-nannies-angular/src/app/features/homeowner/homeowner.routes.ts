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
    { path: 'bookings/:id', loadComponent: () => import('./bookings/booking-detail').then(m => m.BookingDetail)  },
    { path: 'subscriptions', loadComponent: () => import('./subscriptions/my-subscriptions').then(m => m.MySubscriptions) },
    { path: 'jobs', loadComponent: () => import('./jobs/job-list').then(m => m.JobList) },
    { path: 'jobs/create', loadComponent: () => import('./jobs/job-create').then(m => m.JobCreate) },
    { path: 'jobs/:id', loadComponent: () => import('./jobs/job-detail').then(m => m.JobDetail) },
    { path: 'jobs/:id/applications', loadComponent: () => import('./jobs/job-applications').then(m => m.JobApplications) },
];
